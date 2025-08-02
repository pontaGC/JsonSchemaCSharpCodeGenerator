using System;
using System.Threading.Tasks;

namespace JsonSchemaCSharpCodeGenerator
{
    /// <summary>
    /// Retry helper for calling the methods.
    /// </summary>
    public static class RetryHelper
    {
        #region Fields

        // Milliseconds intervals
        private static readonly ushort[] DefaultSpans = { 500, 400, 200 };

        #endregion

        #region Public Methods

        /// <summary>
        /// Invokes an action with retry.
        /// If an exception that occured is a transient error or the invoker wants to retry, retry several times.
        /// </summary>
        /// <param name="action">The invoking action.</param>
        /// <param name="transientExceptionFilter">
        /// The check logic whether the exception thrown is a transient exception.
        /// Retry invoking the action, if the check result is <c>true</c>. Otherwise; throws the exception.
        /// </param>
        /// <param name="retrySpans">The time spans to retry. The unit of them is milliseconds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
        public static void InvokeWithRetry(this Action action, Predicate<Exception> transientExceptionFilter, params ushort[] retrySpans)
        {
            if (transientExceptionFilter is null)
            {
                InvokeWithRetry(action, retrySpans);
                return;
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var intervals = GetIntervalsSafe(retrySpans);
            foreach (var interval in intervals)
            {
                try
                {
                    action.Invoke();
                    return;
                }
                catch (Exception ex)
                {
                    if (!transientExceptionFilter(ex))
                    {
                        // If this isn't a transient error or we shouldn't retry,
                        // rethrow the exception.
                        throw;
                    }

                    Task.Delay(interval).Wait();
                }
            }

            action.Invoke();
        }

        /// <summary>
        /// Invokes an action with retry. If an exception is occured, retry several times.
        /// </summary>
        /// <param name="action">The invoking action.</param>
        /// <param name="retrySpans">The time spans to retry. The unit of them is milliseconds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
        public static void InvokeWithRetry(this Action action, params ushort[] retrySpans)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var intervals = GetIntervalsSafe(retrySpans);
            foreach (var interval in intervals)
            {
                try
                {
                    action.Invoke();
                    return;
                }
                catch (Exception)
                {
                    Task.Delay(interval).Wait();
                }
            }

            action.Invoke();
        }

        /// <summary>
        /// Invokes an action with retry.
        /// If an exception that occured is a transient error or the invoker wants to retry, retry several times.
        /// </summary>
        /// <typeparam name="TResult">The type of invoking the method result.</typeparam>
        /// <param name="execute">The invoking action.</param>
        /// <param name="transientExceptionFilter">
        /// The check logic whether the exception thrown is a transient exception.
        /// Retry invoking the action, if the check result is <c>true</c>. Otherwise; throws the exception.
        /// </param>
        /// <param name="retrySpans">The time spans to retry. The unit of them is milliseconds.</param>
        /// <returns>An execution result, if the execution is success. Otherwise; default value or throws the exception.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="execute"/> is <c>null</c>.</exception>
        public static TResult InvokeWithRetry<TResult>(this Func<TResult> execute, Predicate<Exception> transientExceptionFilter, params ushort[] retrySpans)
        {
            if (transientExceptionFilter is null)
            {
                return InvokeWithRetry(execute, retrySpans);
            }

            if (execute is null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            var intervals = GetIntervalsSafe(retrySpans);
            foreach (var interval in intervals)
            {
                try
                {
                    return execute.Invoke();
                }
                catch (Exception ex)
                {
                    if (!transientExceptionFilter(ex))
                    {
                        // If this isn't a transient error or we shouldn't retry,
                        // rethrow the exception.
                        throw;
                    }

                    Task.Delay(interval).Wait();
                }
            }

            return execute.Invoke();
        }

        /// <summary>
        /// Invokes an action with retry. If an exception is occured, retry several times.
        /// </summary>
        /// <typeparam name="TResult">The type of invoking the method result.</typeparam>
        /// <param name="execute">The invoking action.</param>
        /// <param name="retrySpans">The time spans to retry. The unit of them is milliseconds.</param>
        /// <returns>An execution result, if the execution is success. Otherwise; default value or throws the exception.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="execute"/> is <c>null</c>.</exception>
        public static TResult InvokeWithRetry<TResult>(this Func<TResult> execute, params ushort[] retrySpans)
        {
            if (execute is null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            var intervals = GetIntervalsSafe(retrySpans);
            foreach (var interval in intervals)
            {
                try
                {
                    return execute.Invoke();
                }
                catch (Exception)
                {
                    Task.Delay(interval).Wait();
                }
            }

            return execute.Invoke();
        }

        #region Asynchronously

        /// <summary>
        /// Runs a task with retry. If an exception is occured, retry several times.
        /// </summary>
        /// <param name="taskFactory">The running task factory.</param>
        /// <param name="transientExceptionFilter">
        /// The check logic whether the exception thrown is a transient exception.
        /// Retry invoking the action, if the check result is <c>true</c>. Otherwise; throws the exception.
        /// </param>
        /// <param name="retrySpans">The time spans to retry. The unit of them is milliseconds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="taskFactory"/> is <c>null</c>.</exception>
        public static async Task InvokeAsyncWithRetry(this Func<Task> taskFactory, Predicate<Exception> transientExceptionFilter, params ushort[] retrySpans)
        {
            if (transientExceptionFilter is null)
            {
                await InvokeAsyncWithRetry(taskFactory, retrySpans);
                return;
            }

            if (taskFactory is null)
            {
                throw new ArgumentNullException(nameof(taskFactory));
            }

            var intervals = GetIntervalsSafe(retrySpans);
            foreach (var interval in intervals)
            {
                try
                {
                    await taskFactory.Invoke();
                    return;
                }
                catch (Exception ex)
                {
                    if (!transientExceptionFilter(ex))
                    {
                        // If this isn't a transient error or we shouldn't retry,
                        // rethrow the exception.
                        throw;
                    }

                    await Task.Delay(interval);
                }
            }

            await taskFactory.Invoke();
        }

        /// <summary>
        /// Runs a task with retry. If an exception is occured, retry several times.
        /// </summary>
        /// <param name="taskFactory">The running task factory.</param>
        /// <param name="retrySpans">The time spans to retry. The unit of them is milliseconds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="taskFactory"/> is <c>null</c>.</exception>
        public static async Task InvokeAsyncWithRetry(this Func<Task> taskFactory, params ushort[] retrySpans)
        {
            if (taskFactory is null)
            {
                throw new ArgumentNullException(nameof(taskFactory));
            }

            var intervals = GetIntervalsSafe(retrySpans);
            foreach (var interval in intervals)
            {
                try
                {
                    await taskFactory.Invoke();
                    return;
                }
                catch
                {
                    await Task.Delay(interval);
                }
            }

            await taskFactory.Invoke();
        }

        /// <summary>
        /// Runs a task with retry. If an exception is occured, retry several times.
        /// </summary>
        /// <typeparam name="TResult">The type of invoking the method result.</typeparam>
        /// <param name="taskFactory">The running task factory.</param>
        /// <param name="transientExceptionFilter">
        /// The check logic whether the exception thrown is a transient exception.
        /// Retry invoking the action, if the check result is <c>true</c>. Otherwise; throws the exception.
        /// </param>
        /// <param name="retrySpans">The time spans to retry. The unit of them is milliseconds.</param>
        /// <returns>An execution result, if the execution is success. Otherwise; default value or throws the exception.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="taskFactory"/> is <c>null</c>.</exception>
        public static async Task InvokeAsyncWithRetry<TResult>(this Func<Task<TResult>> taskFactory, Predicate<Exception> transientExceptionFilter, params ushort[] retrySpans)
        {
            if (transientExceptionFilter is null)
            {
                await InvokeAsyncWithRetry(taskFactory, retrySpans);
                return;
            }

            if (taskFactory is null)
            {
                throw new ArgumentNullException(nameof(taskFactory));
            }

            var intervals = GetIntervalsSafe(retrySpans);
            foreach (var interval in intervals)
            {
                try
                {
                    await taskFactory.Invoke();
                    return;
                }
                catch (Exception ex)
                {
                    if (!transientExceptionFilter(ex))
                    {
                        // If this isn't a transient error or we shouldn't retry,
                        // rethrow the exception.
                        throw;
                    }

                    await Task.Delay(interval);
                }
            }

            await taskFactory.Invoke();
        }

        /// <summary>
        /// Runs a task with retry. If an exception is occured, retry several times.
        /// </summary>
        /// <typeparam name="TResult">The type of invoking the method result.</typeparam>
        /// <param name="taskFactory">The running task factory.</param>
        /// <param name="retrySpans">The time spans to retry. The unit of them is milliseconds.</param>
        /// <returns>An execution result, if the execution is success. Otherwise; default value or throws the exception.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="taskFactory"/> is <c>null</c>.</exception>
        public static async Task<TResult> InvokeAsyncWithRetry<TResult>(this Func<Task<TResult>> taskFactory, params ushort[] retrySpans)
        {
            if (taskFactory is null)
            {
                throw new ArgumentNullException(nameof(taskFactory));
            }

            var intervals = GetIntervalsSafe(retrySpans);
            foreach (var interval in intervals)
            {
                try
                {
                    return await taskFactory.Invoke();
                }
                catch
                {
                    await Task.Delay(interval);
                }
            }

            return await taskFactory.Invoke();
        }

        #endregion

        #endregion

        #region Private Methods

        private static ushort[] GetIntervalsSafe(ushort[] specifiedSpans)
        {
            if (specifiedSpans is null || specifiedSpans.Length == 0)
            {
                return DefaultSpans;
            }

            return specifiedSpans;
        }

        #endregion
    }
}
