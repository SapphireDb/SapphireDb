using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileContextCore;
using Microsoft.EntityFrameworkCore;
using RealtimeDatabase.Models.Actions;
using WebUI.Data;
using WebUI.Data.Models;

namespace WebUI.Actions
{
    public class DbActions : ActionHandlerBase
    {
        public static string DbName;

        private readonly RealtimeContext db;

        public DbActions(RealtimeContext db)
        {
            this.db = db;
        }

        public bool testConnection(string key)
        {
            DbContextOptions<SecondRealtimeContext> options = new DbContextOptionsBuilder<SecondRealtimeContext>()
                .UseFileContextDatabase(databaseName: key)
                .Options as DbContextOptions<SecondRealtimeContext>;

            SecondRealtimeContext testDb = new SecondRealtimeContext(options, null);
            return testDb.Database.CanConnect();
        }

        public void updateSettings(string key)
        {
            Config c = db.Configs.FirstOrDefault(cfg => cfg.Key == "DbName");

            if (c != null)
            {
                c.Value = key;
            } else
            {
                c = new Config()
                {
                    Key = "DbName",
                    Value = key
                };
                db.Configs.Add(c);
            }

            db.SaveChanges();

            DbName = key;
        }
    }
}
