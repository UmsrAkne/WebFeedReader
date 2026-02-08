using System;
using System.Threading.Tasks;

namespace WebFeedReader.Api
{
    public interface IFeedSourceSyncService
    {
        Task SyncAsync(DateTimeOffset since);
    }
}