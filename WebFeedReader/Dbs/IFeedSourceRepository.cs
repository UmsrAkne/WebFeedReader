using System.Collections.Generic;
using System.Threading.Tasks;
using WebFeedReader.Models;

namespace WebFeedReader.Dbs
{
    public interface IFeedSourceRepository
    {
        Task UpsertAsync(FeedSource source);

        Task<IReadOnlyList<FeedSource>> GetAllAsync();
    }
}