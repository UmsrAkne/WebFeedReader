using System;
using System.Threading.Tasks;

namespace WebFeedReader.Api
{
    public interface IFeedSourceSyncService
    {
        Task SyncAsync(DateTime since);
    }
}