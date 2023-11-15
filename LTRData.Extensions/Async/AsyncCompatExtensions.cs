﻿#if NET40_OR_GREATER || NETSTANDARD || NETCOREAPP

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace LTRData.Extensions.Async;

/// <summary>
/// </summary>
public static class AsyncCompatExtensions
{

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP
    /// <summary>
    /// A task that has completed with the integer result of 0.
    /// </summary>
    public static Task<int> ZeroCompletedTask { get; } = Task.FromResult(0);

    /// <summary>
    /// A task that has completed with the boolean result of false.
    /// </summary>
    public static Task<bool> FalseResult { get; } = Task.FromResult(false);

    /// <summary>
    /// A task that has completed with the boolean result of true.
    /// </summary>
    public static Task<bool> TrueResult { get; } = Task.FromResult(true);

    private static class Immuatables<T>
    {
        public static readonly Task<T?> DefaultCompletedTask = Task.FromResult<T?>(default);

        public static readonly Task<T[]> EmptyArrayCompletedTask = Task.FromResult(Array.Empty<T>());

        public static readonly Task<IEnumerable<T>> EmptyEnumerationCompletedTask = Task.FromResult(Enumerable.Empty<T>());
    }

    /// <summary>
    /// A task that has completed with the default value of type <typeparamref name="T"/>
    /// </summary>
    public static Task<T?> DefaultCompletedTask<T>() => Immuatables<T>.DefaultCompletedTask;

    /// <summary>
    /// A task that has completed with an empty array of type <typeparamref name="T"/>
    /// </summary>
    public static Task<T[]> EmptyArrayCompletedTask<T>() => Immuatables<T>.EmptyArrayCompletedTask;

    /// <summary>
    /// A task that has completed with an empty enumeration of type <typeparamref name="T"/>
    /// </summary>
    public static Task<IEnumerable<T>> EmptyEnumerationCompletedTask<T>() => Immuatables<T>.EmptyEnumerationCompletedTask;
#endif

#if NET45_OR_GREATER || NETSTANDARD || (NETCOREAPP && !NET6_0_OR_GREATER)
    /// <summary>
    /// </summary>
    /// <param name="task"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task WaitAsync(this Task task, CancellationToken cancellationToken)
        => task.WaitAsync(Timeout.InfiniteTimeSpan, cancellationToken);

    /// <summary>
    /// </summary>
    /// <param name="task"></param>
    /// <param name="timeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async static Task WaitAsync(this Task task, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var timeoutTask = Task.Delay(timeout, cancellationToken);
        var result = await Task.WhenAny(task, timeoutTask).ConfigureAwait(false);
        if (result == timeoutTask)
        {
            throw new TimeoutException();
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="task"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<T> WaitAsync<T>(this Task<T> task, CancellationToken cancellationToken)
        => task.WaitAsync(TimeSpan.FromMilliseconds(-1), cancellationToken);

    /// <summary>
    /// </summary>
    /// <param name="task"></param>
    /// <param name="timeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async static Task<T> WaitAsync<T>(this Task<T> task, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var timeoutTask = Task.Delay(timeout, cancellationToken);
        var result = await Task.WhenAny(task, timeoutTask).ConfigureAwait(false);
        if (result == timeoutTask)
        {
            throw new TimeoutException();
        }

        return task.Result;
    }
#endif

#if NET462_OR_GREATER || NETSTANDARD || NETCOREAPP

    /// <summary>
    /// Waits for a ValueTask to complete, or throws AggregateException if the ValueTask fails. If the ValueTask
    /// has already completed successfully when this method is called, it returns immediately without any further
    /// allocations. Otherwise as Task object is created for waiting and for re-throwing any exceptions etc.
    /// </summary>
    /// <param name="task">ValueTask</param>
    public static void Wait(this in ValueTask task)
    {
        if (!task.IsCompletedSuccessfully)
        {
            task.AsTask().GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Waits for a ValueTask to complete, or throws AggregateException if the ValueTask fails. If the ValueTask
    /// has already completed successfully when this method is called, the result is returned immediately without
    /// any further allocations. Otherwise as Task object is created for waiting for results, exceptions etc.
    /// </summary>
    /// <param name="task">ValueTask</param>
    public static void Wait<T>(this in ValueTask<T> task)
    {
        if (!task.IsCompletedSuccessfully)
        {
            task.AsTask().GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Waits for a ValueTask to complete, or throws AggregateException if the ValueTask fails. If the ValueTask
    /// has already completed successfully when this method is called, the result is returned immediately without
    /// any further allocations. Otherwise as Task object is created for waiting for results, exceptions etc.
    /// </summary>
    /// <param name="task">ValueTask</param>
    public static T WaitForResult<T>(this in ValueTask<T> task)
    {
        if (task.IsCompletedSuccessfully)
        {
            return task.Result;
        }
        else
        {
            return task.AsTask().GetAwaiter().GetResult();
        }
    }

#endif

#if NET45_OR_GREATER || NETSTANDARD || NETCOREAPP

    /// <summary>
    /// </summary>
    public static IAsyncResult AsAsyncResult<T>(this Task<T> task, AsyncCallback? callback, object? state)
    {
        var returntask = task.ContinueWith((t, _) => t.Result, state, TaskScheduler.Default);

        if (callback is not null)
        {
            returntask.ContinueWith(callback.Invoke, TaskScheduler.Default);
        }

        return returntask;
    }

    /// <summary>
    /// </summary>
    public static IAsyncResult AsAsyncResult(this Task task, AsyncCallback? callback, object? state)
    {
        var returntask = task.ContinueWith((t, _) => { }, state, TaskScheduler.Default);

        if (callback is not null)
        {
            returntask.ContinueWith(callback.Invoke, TaskScheduler.Default);
        }

        return returntask;
    }

#endif

}

#endif
