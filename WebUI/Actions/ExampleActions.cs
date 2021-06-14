using SapphireDb.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.FileSystemGlobbing;
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

        // public async Task TestException()
        // {
        //     throw new Exception("Should throw in client");
        // }
        
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

        public string GuidTest(Guid test)
        {
            return test.ToString();
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

        public async IAsyncEnumerable<string> StreamTest(IAsyncEnumerable<string> inputStream)
        {
            await foreach (string input in inputStream)
            {
                yield return input + " from server";
            }
        }
        
        public bool MatchesGlobPattern(string input, string globPattern)
        {
            Matcher m = new Matcher();
            m.AddInclude(globPattern);
            return m.Match(input).HasMatches;
        }
    }
}
