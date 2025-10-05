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

namespace LughSharp.Lugh.Utils;

/// <summary>
/// Allows asynchronous execution of IAsyncTask instances on a separate thread.
/// Needs to be disposed via a call to Dispose() when no longer used, in which case the executor waits for running tasks to finish.
/// Scheduled but not yet running tasks will not be executed.
/// </summary>
[PublicAPI]
public class AsyncExecutor : IDisposable
{
    // The C# TaskScheduler is typically used to manage where Tasks run.
    // For a fixed-size thread pool, we can use a custom TaskScheduler or a SemaphoreSlim
    // to limit concurrency on the default TaskScheduler, but for simplicity,
    // we'll use a SemaphoreSlim to manage the worker threads' capacity
    // and let the built-in TaskFactory handle the actual task creation.

    private readonly SemaphoreSlim           _semaphore;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly List< Task >            _runningTasks            = [ ];
    private          bool                    _isDisposed              = false;

    // ========================================================================

    /// <summary>
    /// Creates a new AsyncExecutor that allows maxConcurrent tasks to run in parallel.
    /// </summary>
    /// <param name="maxConcurrent">The maximum number of tasks to run concurrently.</param>
    /// <param name="name">The name for the threads (used here for logging/debugging context, though C# threads are harder to name globally)</param>
    public AsyncExecutor( int maxConcurrent, string name = "AsyncExecutor-Thread" )
    {
        // The thread naming from the Java example is less direct in C# Task model,
        // but we can manage the degree of parallelism.
        if ( maxConcurrent <= 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( maxConcurrent ), "maxConcurrent must be greater than zero." );
        }

        _semaphore = new SemaphoreSlim( maxConcurrent, maxConcurrent );

        // Note: The thread name is mostly ignored in this C# implementation
        // as Tasks use the default ThreadPool.
    }

    /// <summary>
    /// Submits an IAsyncTask to be executed asynchronously. If maxConcurrent
    /// tasks are already running, the task will be queued.
    /// </summary>
    /// <param name="task">the task to execute asynchronously</param>
    public AsyncResult< T > Submit< T >( IAsyncTask< T > task )
    {
        if ( _isDisposed )
        {
            throw new GdxRuntimeException( "Cannot run tasks on an executor that has been shutdown (disposed)" );
        }

        // We use Task.Run for simple queuing onto the thread pool, 
        // and wrap the execution logic with the SemaphoreSlim.
        var cts = CancellationTokenSource.CreateLinkedTokenSource( _cancellationTokenSource.Token );

        var taskWrapper = Task.Run( async () =>
        {
            // Wait for a slot in the concurrency limit
            await _semaphore.WaitAsync( cts.Token );

            try
            {
                // Check if cancellation was requested while waiting (i.e., Dispose was called)
                if ( cts.Token.IsCancellationRequested )
                {
                    // Throw to mark the task as canceled
                    cts.Token.ThrowIfCancellationRequested();
                }

                // Execute the user's task logic
                return task.Call();
            }
            finally
            {
                // Release the slot back to the semaphore
                _semaphore.Release();
            }
        }, cts.Token );

        // Track the task so Dispose can await it
        lock ( _runningTasks )
        {
            _runningTasks.Add( taskWrapper );

            // Optionally remove the task when it completes, but for Dispose() simplicity, 
            // we'll just await all tracked tasks.
        }

        return new AsyncResult< T >( taskWrapper );
    }

    /// <summary>
    /// Waits for running IAsyncTask instances to finish, then destroys any resources like threads.
    /// Can not be used after this method is called.
    /// </summary>
    public void Dispose()
    {
        if ( _isDisposed ) return;

        _isDisposed = true;

        // 1. Signal cancellation for any tasks waiting in the semaphore queue
        // The semaphore.WaitAsync logic above handles the check.
        _cancellationTokenSource.Cancel();

        // 2. Wait for all currently running tasks to finish.
        // We capture a list of current tasks and wait for them all.
        Task[] tasksToWait;

        lock ( _runningTasks )
        {
            tasksToWait = _runningTasks.ToArray();
        }

        try
        {
            // Use Task.WaitAll with a large timeout, similar to the Long.MAX_VALUE, TimeUnit.SECONDS in Java.
            // C# tasks don't have a direct equivalent to Java's executor.awaitTermination, but Task.WaitAll works
            // for waiting for the active work to complete.
            Task.WaitAll( tasksToWait, Timeout.Infinite );
        }
        catch ( AggregateException ae )
        {
            // The original Java code re-throws InterruptedException as GdxRuntimeException.
            // We'll re-throw a GdxRuntimeException if any of the underlying tasks failed or if WaitAll itself was interrupted
            // (e.g., if we were to use a timeout and it elapsed, but we are using Infinite here).
            // A more robust pattern would check for ThreadAbortException or TaskCanceledException.
            throw new GdxRuntimeException( "Couldn't shutdown loading thread gracefully.", ae );
        }
        finally
        {
            _semaphore.Dispose();
            _cancellationTokenSource.Dispose();

            // Clear the list after waiting
            lock ( _runningTasks )
            {
                _runningTasks.Clear();
            }
        }
    }
}

// ========================================================================
// ========================================================================