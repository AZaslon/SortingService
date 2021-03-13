
using System;
using System.Net.Http;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace SortingWebApi.Common
{
    /// <inheritdoc />
    public class RetryPolicyFactory : IRetryPolicyFactory
    {
        //TODO: move to configuration 
        private readonly int _attempts = 2;
        private readonly TimeSpan _waitInterval = TimeSpan.FromSeconds(1);


        /// <inheritdoc />
        public AsyncRetryPolicy<T> Create<T, THandledException>(ILogger logger) where THandledException : Exception
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            return Policy<T>
                .Handle<THandledException>(ex => !(ex is OperationCanceledException))
                .Or<HttpRequestException>(ex => ex.InnerException is SocketException)
                .Or<SocketException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    _attempts,
                    retryAttempt => _waitInterval,
                    (delegateResult, timeSpan, retryCount, context) =>
                    {
                        logger.LogDebug(delegateResult.Exception, "Retry attempt: {0}", retryCount);
                    });
        }

        /// <inheritdoc />
        public AsyncRetryPolicy Create<THandledException>(ILogger logger) where THandledException : Exception
        {
            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            return Policy
                .Handle<THandledException>(ex => !(ex is OperationCanceledException))
                .Or<HttpRequestException>(ex => ex.InnerException is SocketException)
                .Or<SocketException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    _attempts,
                    retryAttempt => _waitInterval,
                    (delegateResult, timeSpan, retryCount, context) =>
                    {
                        logger.LogDebug(delegateResult.InnerException, "Retry attempt: {0}", retryCount);
                    });
        }
    }
}

