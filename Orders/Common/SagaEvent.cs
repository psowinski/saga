using System;

namespace Domain.Common
{
   public abstract class SagaEvent<T> : Event where T : State
   {
      protected SagaEvent()
      {
      }

      protected SagaEvent(State state, Command command) : base(state, command)
      {
         CorrelationId = command.CorrelationId;
         TimeStamp = command.TimeStamp;
      }

      public string CorrelationId { get; set; }
      public DateTime TimeStamp { get; set; }
   }
}
