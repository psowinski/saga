using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Order;
using Domain.Payment;
using Domain.Dispatch;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Saga;
using Persistence;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace ShoppingSaga
{
   public class Worker : BackgroundService
   {
      private readonly ILogger<Worker> logger;
      private readonly Saga.Saga saga;
      private readonly AppClient appClient;
      private readonly PersistenceClient persistence;

      private bool disposed = false;
      public override void Dispose()
      {
         if(!disposed)
         {
            this.persistence.Dispose();
            this.appClient.Dispose();
            disposed = true;
         }
         base.Dispose();
      }

      public Worker(ILogger<Worker> logger, IConfiguration cfg, IHostEnvironment env)
      {
         this.logger = logger;

         var appUrl = cfg.GetValue<string>("AppUrl");
         var persistenceUrl = cfg.GetValue<string>("PersistenceUrl");

         var isDev = env.IsDevelopment();

         this.appClient = new AppClient(appUrl);
         this.persistence = new PersistenceClient(persistenceUrl);

         var shopping = new SagaConfiguration()
            .OnEvent(Protect((OrderCheckedOut evn) => appClient.Pay(evn.StreamId, evn.CorrelationId)))
            .OnEvent(Protect((PaymentRequested evn) => appClient.FinalizePayment(evn.StreamId, evn.CorrelationId, evn.Total, evn.Description)))
            .OnEvent(Protect((OrderPaid evn) => appClient.Dispatch(evn.OrderStreamId, evn.StreamId, evn.CorrelationId)))
            .EndOnEvent<OrderDispatched>();

         this.saga = new Saga.Saga(shopping, persistence, this.logger);
      }

      private List<Exception> failures = new List<Exception>();

      private Func<TEvent, Task> Protect<TEvent>(Func<TEvent, Task> action)
      {
         async Task safe(TEvent evn)
         {
            try
            {
               await action(evn);
            }
            catch (Exception e)
            {
               this.failures.Add(e);
            }
         }
         return safe;
      }

      private void LogActionFailures()
      {
         this.failures.ForEach(x => this.logger.LogError(x.ToString()));
         this.failures.Clear();
      }

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
         var stopwatch = new Stopwatch();
         while (!stoppingToken.IsCancellationRequested)
         {
            stopwatch.Restart();
            try
            {
               await this.saga.Run();
               LogActionFailures();
            }
            catch (Exception e)
            {
               this.logger.LogError(e.ToString());
            }
            var delta = 50 - stopwatch.ElapsedMilliseconds;
            if(delta > 0)
               await Task.Delay((int)delta, stoppingToken);
         }
      }
   }
}
