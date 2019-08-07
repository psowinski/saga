using System;
using System.Threading.Tasks;

namespace Runner
{
   public class ServiceSimulator<T>
   {
      private readonly Func<IServiceTask> serviceFactory;
      private readonly Network network = new Network();
      private readonly Random rnd = new Random();

      private IServiceTask serviceTask;
      private bool restoredAfterCrash;
      private bool onNetwork;

      public ServiceSimulator(Func<IServiceTask> serviceFactory)
      {
         this.serviceFactory = serviceFactory;
         this.serviceTask = serviceFactory();
      }

      public async Task Start()
      {
         var idx = 0;
         while (true)
         {
            await Task.Delay(500);

            await CrashAndRestartService();

            this.onNetwork = this.network.ReceiveLowValueMessage();
            var onTime = ++idx % 5 == 0;

            if (this.onNetwork || onTime)
            {
               PrintStatusMessages();
               await this.serviceTask.Run();
            }
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
         this.serviceTask = this.serviceFactory();
         this.restoredAfterCrash = true;
      }

      private void PrintStatusMessages()
      {
         if (this.restoredAfterCrash)
         {
            this.restoredAfterCrash = false;
            Console.WriteLine("-= Saga Manager restored after crash =-");
         }
         else
         {
            var why = this.onNetwork ? "network" : "timer";
            Console.WriteLine($"Saga Manager was awakened by a [{why}].");
         }
      }
   }
}