namespace Infrastructure
{
   public class Network
   {
      private static readonly NetworkDriver NetworkDriver = new NetworkDriver();

      public void SendSagaWakeup() => NetworkDriver.SendSagaWakeup();

      public bool ReceiveSagaWakeup() => NetworkDriver.ReceiveSagaWakeup();
   }
}
