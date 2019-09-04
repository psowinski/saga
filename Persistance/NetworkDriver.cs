﻿using System;

namespace Infrastructure
{
   public class NetworkDriver
   {
      private readonly object dataLock = new object();
      private readonly Random rnd = new Random();
      private bool message;

      public void SendSagaWakeup()
      {
         lock (this.dataLock)
         {
            this.message = true;
         }
      }

      public int LostMessageProbability = 50;

      public bool ReceiveSagaWakeup()
      {
         lock (this.dataLock)
         {
            if (!this.message) return false;
            this.message = false;
            var ret = rnd.Next(0, 99) < LostMessageProbability;
            if(!ret) Console.WriteLine($"Network message lost. Network loose {LostMessageProbability}% of messages.");
            return ret;
         }
      }
   }
}