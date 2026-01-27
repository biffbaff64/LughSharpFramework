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

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Utils;

/// <summary>
/// Returned by AsyncExecutor.Submit(IAsyncTask&lt;T&gt;), allows to poll for
/// the result of the asynchronous workload.
/// </summary>
/// <typeparam name="T">The result type of the asynchronous task.</typeparam>
[PublicAPI]
public class AsyncResult< T >
{
    private readonly Task< T? > _task;

    // ========================================================================

    /// <summary>
    /// Initializes a new instance of the AsyncResult class, wrapping a C# Task.
    /// </summary>
    /// <param name="task">The underlying Task representing the asynchronous operation.</param>
    public AsyncResult( Task< T? > task )
    {
        this._task = task ?? throw new ArgumentNullException( nameof( task ) );
    }

    /// <summary>
    /// Checks whether the task is done.
    /// </summary>
    /// <returns>
    /// True if the task is done (completed, faulted, or canceled), otherwise false.
    /// </returns>
    public bool IsDone()
    {
        return _task.IsCompleted;
    }

    /// <summary>
    /// Waits if necessary for the computation to complete and then returns the result.
    /// </summary>
    /// <returns>The result of the asynchronous computation.</returns>
    /// <exception cref="RuntimeException">If there was an error during the task execution.</exception>
    public T? Get()
    {
        try
        {
            // Task<T>.Result blocks the calling thread until the task is complete.
            // If the task failed, it throws an AggregateException.
            return _task.Result;
        }
        catch ( AggregateException ex )
        {
            // C#'s Task<T>.Result throws an AggregateException, which wraps the inner
            // exception(s). We unwrap the cause and throw a RuntimeException.

            // Get the first inner exception that caused the fault.
            var innerException = ex.InnerException;

            // Note: If the task was canceled, AggregateException wraps a TaskCanceledException.
            // We will re-throw the actual cause.

            if ( innerException != null )
            {
                throw new RuntimeException( innerException );
            }

            // If there's no inner exception (unlikely for a faulted task), throw
            // the AggregateException itself wrapped.
            throw new RuntimeException( ex );
        }

        // In C#, if a thread is interrupted/canceled while blocking on .Result, a TaskCanceledException
        // (or OperationCanceledException) is usually wrapped inside the AggregateException,
        // which is handled above.
    }
}

// ========================================================================
// ========================================================================