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

        Task MarkAsReadAsync(IEnumerable<string> keys);
    }
}