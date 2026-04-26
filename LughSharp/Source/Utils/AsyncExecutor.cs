// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Circa64 Software Projects
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

using System;
using System.Threading;
using System.Threading.Tasks;

using LughSharp.Source.Assets;
using LughSharp.Source.Utils.Exceptions;

namespace LughSharp.Source.Utils;

public class AsyncExecutor : IDisposable
{
    // C# mechanism to limit the number of concurrent tasks (the maxConcurrent property)
    private readonly SemaphoreSlim _semaphore;

    private bool _isDisposed;

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
        if ( _isDisposed )
        {
            throw new RuntimeException( "Cannot run tasks on an executor that has been shut down (disposed)" );
        }

        // Wait for a concurrency slot. Use WaitAsync() to avoid blocking the calling thread.
        // Use ConfigureAwait(false) to avoid context switching issues.
        //@formatter:off
        Task submissionTask = _semaphore.WaitAsync().ContinueWith( waitTask =>
        {
           // If the wait was successful, execute the task.
           if ( waitTask.IsCompletedSuccessfully )
           {
               // Task.Run executes the work on the standard .NET ThreadPool.
               return Task.Run( () =>
               {
                   try
                   {
                       task.Call();
                   }
                   catch ( Exception e )
                   {
                       // Wrap and rethrow as RuntimeException if needed, or just let it bubble up
                       throw new RuntimeException( "Asynchronous task failed.", e );
                   }
                   finally
                   {
                       // Release the semaphore slot when the work is finished (SUCCESS or FAILURE)
                       _semaphore.Release();
                   }
               } );
           }

           return Task.CompletedTask;
        },
        // Unwrap the Task<Task> to return just the inner Task
        TaskContinuationOptions.ExecuteSynchronously ).Unwrap();
        //@formatter:on
        
        return submissionTask;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing,
    /// or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    private void Dispose( bool disposing )
    {
        if ( !_isDisposed )
        {
            if ( disposing )
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