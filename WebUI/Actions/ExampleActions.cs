using RealtimeDatabase.Attributes;
using RealtimeDatabase.Models.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebUI.Data;

namespace WebUI.Actions
{
    public class ExampleActions : ActionHandlerBase
    {
        private readonly RealtimeContext db;

        public ExampleActions(RealtimeContext _db)
        {
            db = _db;
        }

        [ActionAuth]
        public int GenerateRandomNumber()
        {
            for (int i = 0; i <= 100; i++)
            {
                Thread.Sleep(10);
                Notify("Progress: " + i + "%");
            }

            return db.Users.Count();
        }

        public string TestWithParams(string param1, string param2)
        {
            return param1 + param2;
        }

        public void NoReturn()
        {
            Console.WriteLine("This is a test");
        }
    }
}
