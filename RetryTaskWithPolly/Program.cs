using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace RetryTaskWithPolly
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var policy = Policy.Handle<Exception>()
                   //.RetryAsync(3, (exception, context) => Console.WriteLine($"{exception.Message}"));
                   //.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(1));
                   .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(1));                             

            List<Task<string>> tasks = new List<Task<string>>();
            foreach (var itm in new int[3] {1,2,3})
            {
                var tsk = policy.ExecuteAsync(() => DoSomething(itm));
                tasks.Add(tsk);
            }
            while (tasks.Any())
            {
                Task<string> completedTask = await Task.WhenAny(tasks);
                if (completedTask.IsCompleted)
                {
                    try
                    {
                        Console.WriteLine(completedTask.Result);
                        tasks.Remove(completedTask);
                    }
                    catch (Exception ex){}
                }
            }
            Console.WriteLine("All Done..!");
            Console.ReadLine();
        }


        public static async Task<string> DoSomething(int taskno)
        {
            if (taskno == 2) await Task.Delay(1000);

            Ping ping = new Ping();
            PingReply pingStatus = ping.Send(IPAddress.Parse("8.8.8.8"));

            if (pingStatus.Status != IPStatus.Success)
            {
                Console.WriteLine($"Task {taskno} is failed..!");
                throw new Exception($"Task {taskno} is failed..!");
            }
            return await Task.FromResult($"Task {taskno} is done..!");
        }
    }
}