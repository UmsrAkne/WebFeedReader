using System.Collections.Generic;
using System.Threading.Tasks;
using WebFeedReader.Models;

namespace WebFeedReader.Dbs
{
    public interface IFeedItemRepository
    {
        Task UpsertAsync(FeedItem item);

        Task<IReadOnlyList<FeedItem>> GetAllAsync();

        Task<IReadOnlyList<FeedItem>> GetBySourceIdAsync(int sourceId);

        Task<IReadOnlyList<FeedItem>> GetBySourceIdPagedAsync(int sourceId, int offset, int limit);

        Task MarkAsReadAsync(IEnumerable<string> keys);

        Task ApplyNgCheckResultsAsync(IEnumerable<NgCheckResult> results);
    }
}