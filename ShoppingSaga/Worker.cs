using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Saga;
using Persistence;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Common.Aggregate;
using Domain.Model.Dispatch;
using Domain.Model.Order;
using Domain.Model.Payment;

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

      private readonly List<string> errors = new List<string>();

      public Worker(ILogger<Worker> logger, IConfiguration cfg, IHostEnvironment env)
      {
         this.logger = logger;

         var appUrl = cfg.GetValue<string>("AppUrl");
         var persistenceUrl = cfg.GetValue<string>("PersistenceUrl");

         var isDev = env.IsDevelopment();

         this.appClient = new AppClient(appUrl);
         this.persistence = new PersistenceClient(persistenceUrl, new PaymentEventUpdater());

         Func<TEvent, Task<ActionStatus>> Protect<TEvent>(Func<TEvent, Task<ActionStatus>> action) where TEvent : Event
            => SagaUtil.Protect(action, this.errors);

         var shopping = new SagaConfiguration("Shopping")
            .OnEvent(Protect((OrderCheckedOut evn) => appClient.Pay(evn.StreamId, evn.CorrelationId)))
            .OnEvent(Protect((PaymentRequested evn) => appClient.FinalizePayment(evn.StreamId, evn.CorrelationId, evn.Total, evn.Description)))
            .OnEvent(Protect((PaymentCompleted evn) => appClient.Dispatch(evn.OrderStreamId, evn.StreamId, evn.CorrelationId)))
            .EndOnEvent<PaymentCancelled>()
            .EndOnEvent<OrderDispatched>();

         this.saga = new Saga.Saga(shopping, persistence, this.logger);
      }

      private void LogActionErrors()
      {
         this.errors.ForEach(x => this.logger.LogError(x));
         this.errors.Clear();
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
               LogActionErrors();
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
