using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetLearningForTasks
{
    class Program
    {
        static int Main(string[] args)
        {
            //  Doing basic stuff with Tasks

            Console.WriteLine("Running task from thread " + Thread.CurrentThread.ManagedThreadId);

            GetTaskWhichWritesThreadIdToConsole("synchronous").RunSynchronously();

            var async = GetTaskWhichWritesThreadIdToConsole("asynchronous1");
            var async2 = GetTaskWhichWritesThreadIdToConsole("asynchronous2");
            async.Start();
            GetTaskWhichWritesThreadIdToConsole("asynchronous1b").Start();
            async.ContinueWith(t => async2.RunSynchronously());
            async2.Wait();

            // Wrapping IAsyncResult usage with a Task using Task,Factory.FromAsync

            Task<IPAddress[]> googleTask = Task<IPAddress[]>.Factory.FromAsync(Dns.BeginGetHostAddresses("google.com", null, null), Dns.EndGetHostAddresses);

            //  Note that we do not start this one, we just wait for it.
            googleTask.Wait();

            Console.WriteLine("IP address of google.com is " + string.Join(", ", googleTask.Result.Select(i => i.ToString())));

            // Wrapping IAsyncResult usage with a Task manually (using TaskCompletionsource)

            var bingTask = GetTaskForGetIPAddresses("bing.com");

            //  Note that we do not start this one, we just wait for it.
            bingTask.Wait();

            Console.WriteLine("IP address of bing.com is " + string.Join(", ", bingTask.Result.Select(i => i.ToString())));

            return 0;
        }

        private static Task GetTaskWhichWritesThreadIdToConsole(string name )
        {
            return new Task(() => { Console.WriteLine(name + " -- Task ran on thread " + Thread.CurrentThread.ManagedThreadId); });
        }

        private static Task<IPAddress[]> GetTaskForGetIPAddresses(string hostname)
        {
            var tcs = new TaskCompletionSource<IPAddress[]>();

            Dns.BeginGetHostAddresses(hostname, iar =>
            {
                try
                {
                    tcs.SetResult(Dns.EndGetHostAddresses(iar));
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                    throw;
                }
            }, null);

            return tcs.Task;
        }
    }
}
