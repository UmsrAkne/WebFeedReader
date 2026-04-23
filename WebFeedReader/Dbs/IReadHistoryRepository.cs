using System.Collections.Generic;
using System.Threading.Tasks;
using WebFeedReader.Models;

namespace WebFeedReader.Dbs
{
    public interface IReadHistoryRepository
    {
        Task AddAsync(ReadHistory history);

        Task<IReadOnlyList<ReadHistory>> GetAllAsync();
    }
}