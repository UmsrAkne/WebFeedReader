using System;
using System.Threading.Tasks;

namespace WebFeedReader.Api
{
    public interface IFeedSyncService
    {
        Task SyncAsync(DateTime since);
    }
}