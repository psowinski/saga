using System;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
   public class ServiceSimulator<T>
   {
      private readonly Func<ITask> taskFactory;
      private readonly string name;
      private readonly Network network = new Network();
      private readonly Random rnd = new Random();

      private ITask serviceTask;
      private bool restoredAfterCrash;
      private bool onNetwork;

      public ServiceSimulator(Func<ITask> taskFactory, string name)
      {
         this.taskFactory = taskFactory;
         this.name = name;
         this.serviceTask = taskFactory();
      }

      public async Task Start()
      {
         var idx = 0;
         while (true)
         {
            await Task.Delay(500);

            await CrashAndRestartService();

            this.onNetwork = this.network.ReceiveSagaWakeup();
            var onTime = ++idx % 5 == 0;

            if (this.onNetwork || onTime)
            {
               PrintStatusMessages();
               await RunTask();
            }
         }
      }

      private async Task RunTask()
      {
         try
         {
            await this.serviceTask.Run();
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
            throw;
         }
      }

      private async Task CrashAndRestartService()
      {
         if (DidServiceCrash())
            await SimulateCrashAndRestart();
      }

      private bool DidServiceCrash()
         => rnd.Next(99) < 10;

      private async Task SimulateCrashAndRestart()
      {
         await Task.Delay(2000);
         this.serviceTask = this.taskFactory();
         this.restoredAfterCrash = true;
      }

      private void PrintStatusMessages()
      {
         if (this.restoredAfterCrash)
         {
            this.restoredAfterCrash = false;
            Console.WriteLine($"-= Service '{this.name}' restored after crash =-");
         }
         else
         {
            var why = this.onNetwork ? "network" : "timer";
            Console.WriteLine($"Service '{this.name}' was awakened by a [{why}].");
         }
      }
   }
}