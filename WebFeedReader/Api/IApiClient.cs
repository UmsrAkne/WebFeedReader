using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebFeedReader.Models;

namespace WebFeedReader.Api
{
    public interface IApiClient : IDisposable
    {
        Task<string> GetFeedsAsync(DateTimeOffset since, CancellationToken ct = default);

        Task<string> GetSourcesAsync(DateTimeOffset since, CancellationToken ct = default);

        Task CreateSourceAsync(SourceCreateRequest request, CancellationToken ct = default);

        Task<IEnumerable<NgWord>> GetNgWordsAsync(int lastVersion);

        Task PostReadStatusAsync(IEnumerable<string> feedItemKeys);
    }
}