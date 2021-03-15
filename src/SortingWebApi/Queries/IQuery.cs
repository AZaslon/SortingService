using System.Collections.Generic;
using System.Threading;

namespace SortingWebApi.Queries
{
    /// <summary>
    /// Interface for query implementation 
    /// </summary>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <typeparam name="TResponse">Type of Collection of response items.</typeparam>
    public interface IQuery<in TRequest, out TResponse>
    {
        TResponse ExecuteRequest(TRequest request, CancellationToken cancellationToken);
    }
}