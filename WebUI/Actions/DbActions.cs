using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileContextCore;
using Microsoft.EntityFrameworkCore;
using SapphireDb.Actions;
using WebUI.Data;
using WebUI.Data.DemoDb;
using WebUI.Data.Models;

namespace WebUI.Actions
{
    public class DbActions : ActionHandlerBase
    {
        public static string DbName;

        private readonly RealtimeContext db;
        private readonly DemoContext demoDb;

        public DbActions(RealtimeContext db, DemoContext demoDb)
        {
            this.db = db;
            this.demoDb = demoDb;
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
