using System;
using Microsoft.Extensions.Logging;
using Polly.Retry;

namespace SortingWebApi.Common
{
 
    /// <summary>
    /// Interface for retry policy.
    /// </summary>
    public interface IRetryPolicyFactory
    {
        /// <summary>
        /// Returns instance of Retry policy.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <typeparam name="T">Result type.</typeparam>
        /// <typeparam name="THandledException">Handled exception type.</typeparam>
        /// <returns>Returns retry policy.</returns>
        AsyncRetryPolicy<T> Create<T, THandledException>(ILogger logger) where THandledException : Exception;

        /// <summary>
        /// Returns instance of Retry policy.
        /// </summary>
        /// <typeparam name="THandledException">Handled exception type.</typeparam>
        /// <param name="logger">Logger.</param>
        /// <returns>Returns retry policy.</returns>
        AsyncRetryPolicy Create<THandledException>(ILogger logger) where THandledException : Exception;
    }
}
