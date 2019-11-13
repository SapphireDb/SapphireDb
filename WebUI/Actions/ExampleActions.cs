using RealtimeDatabase.Attributes;
using RealtimeDatabase.Models.Actions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebUI.Data;

namespace WebUI.Actions
{
    public class ExampleActions : ActionHandlerBase
    {
        private readonly RealtimeContext db;

        public ExampleActions(RealtimeContext db)
        {
            this.db = db;
        }

        public async Task<int> GenerateRandomNumber()
        {
            for (int i = 0; i <= 100; i++)
            {
                Thread.Sleep(10);
                Notify(i);
            }

            await Task.Delay(1000);

            return db.Users.Count();
        }

        public async Task<string> AsyncDelay()
        {
            for (int i = 0; i <= 100; i++)
            {
                await Task.Delay(10);
                Notify(i);
            }

            return "complete";
        }

        //public bool test(WebsocketConnection connection)
        //{
        //    return DateTime.UtcNow.Second % 2 == 0;
        //}

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
