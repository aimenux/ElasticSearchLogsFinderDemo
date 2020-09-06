using System.Collections.Generic;
using System.Threading;

namespace Lib.ElasticSearch
{
    public interface IElasticProvider
    {
        IAsyncEnumerable<T> QueryAsync<T>(CancellationToken cancellationToken = default) where T : class;
    }
}
