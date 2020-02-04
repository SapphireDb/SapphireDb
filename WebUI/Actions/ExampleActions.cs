using SapphireDb.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SapphireDb.Actions;
using WebUI.Data;

namespace WebUI.Actions
{
    [ActionAuth(functionName: "Test")]
    public class ExampleActions : ActionHandlerBase
    {
        public async Task<int> GenerateRandomNumber()
        {
            for (int i = 0; i <= 100; i++)
            {
                Thread.Sleep(10);
                Notify(i);
            }

            await Task.Delay(1000);

            return 15;
        }

        [ActionAuth(functionName: "Test")]
        public async Task<string> AsyncDelay()
        {
            for (int i = 0; i <= 100; i++)
            {
                await Task.Delay(10);
                Notify(i);
            }

            return "complete";
        }

        public bool Test()
        {
            return true;
        }
        
        public string TestWithParams(string param1, string param2)
        {
            return param1 + param2;
        }

        public void NoReturn()
        {
            Console.WriteLine("This is a test");
        }

        public string CreateHash(string input)
        {
            return input.ComputeHash();
        }

        public async IAsyncEnumerable<string> AsyncEnumerableTest()
        {
            for (int i = 0; i <= 100; i++)
            {
                yield return i.ToString();

                if (i % 10 == 0)
                {
                    Notify("Progress: " + i + "%");
                }
                
                await Task.Delay(10);
            }
        }
    }
}
