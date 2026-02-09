using System;
using System.Threading.Tasks;

namespace WebFeedReader.Api
{
    public interface IFeedSyncService
    {
        Task SyncAsync(DateTimeOffset since);
    }
}