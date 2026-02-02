using Microsoft.EntityFrameworkCore;

namespace WebFeedReader.Dbs
{
    public static class DatabaseInitializer
    {
        public static void EnsureDatabase(DbContext context)
        {
            context.Database.EnsureCreated();
        }
    }
}