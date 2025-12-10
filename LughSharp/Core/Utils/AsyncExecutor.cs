// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// /////////////////////////////////////////////////////////////////////////////

using LughSharp.Core.Assets;

namespace LughSharp.Core.Utils;

public class AsyncExecutor : IDisposable
{
    // C# mechanism to limit the number of concurrent tasks (the maxConcurrent property)
    private readonly SemaphoreSlim _semaphore;

    private bool _isDisposed = false;

    // ========================================================================

    public AsyncExecutor( int maxConcurrent, string name = "" )
    {
        // Semaphore initialized with maxConcurrent concurrent slots
        
        _semaphore = new SemaphoreSlim( maxConcurrent, maxConcurrent );
        // Thread naming is often ignored in .NET, relying on debugger tools, but name
        // is used here for logging and debugging purposes.
    }

    /// <summary>
    /// Submits a task to be executed asynchronously, limiting concurrency via a SemaphoreSlim.
    /// </summary>
    /// <param name="task"> The AssetLoadingTask instance that implements the Call() method. </param>
    /// <returns> A Task that can be used like the original AsyncResult. </returns>
    public Task Submit( IAssetTask task ) // No generics needed since AssetLoadingTask returns void
    {
        if (_isDisposed)
        {
            throw new GdxRuntimeException( "Cannot run tasks on an executor that has been shut down (disposed)" );
        }

        // 1. Wait for a concurrency slot. Use WaitAsync() to avoid blocking the calling thread.
        // We use ConfigureAwait(false) for library code to avoid context switching issues.
        var submissionTask = _semaphore.WaitAsync().ContinueWith(waitTask =>
        {
            // If the wait was successful, execute the task.
            if (waitTask.IsCompletedSuccessfully)
            {
                // 2. Task.Run executes the work on the standard .NET ThreadPool.
                return Task.Run(() =>
                {
                    try
                    {
                        task.Call();
                    }
                    catch (Exception e)
                    {
                        // Wrap and rethrow as GdxRuntimeException if needed, or just let it bubble up
                        throw new GdxRuntimeException("Asynchronous task failed.", e);
                    }
                    finally
                    {
                        // 3. Release the semaphore slot when the work is finished (SUCCESS or FAILURE)
                        _semaphore.Release();
                    }
                });
            }
            return Task.CompletedTask;
        }, TaskContinuationOptions.ExecuteSynchronously)
        .Unwrap(); // Unwrap the Task<Task> to return just the inner Task

        return submissionTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_isDisposed) return;

        Dispose( true );
        GC.SuppressFinalize( this );
    }

    /// <inheritdoc cref="Dispose"/>
    protected void Dispose( bool disposing )
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // In the SemaphoreSlim model, waiting for all tasks is difficult without
                // tracking them manually. For simplicity, we mostly rely on the GC and the
                // OS to clean up the Task threads, after preventing new tasks from starting.

                // TODO: If it turns out I absolutely need to wait for all *currently running* tasks,
                // I will need to implement a more complex tracking mechanism. This would need a
                // concurrent collection to track all the Tasks returned by Submit().
                // As it stands currently, relying on the semaphore to prevent *new* tasks is often enough.

                _semaphore.Dispose();
            }

            _isDisposed = true;
        }
    }
}

// ========================================================================
// ========================================================================