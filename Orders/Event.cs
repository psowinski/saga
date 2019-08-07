﻿using System;

namespace Domain
{
   public abstract class Event
   {
      public string CorrelationId;
      public DateTime TimeStamp;

      public string StreamId = "";
      public int Version = 0;
   }
}