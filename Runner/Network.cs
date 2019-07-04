namespace Runner
{
   public class Network
   {
      private static readonly NetworkDriver NetworkDriver = new NetworkDriver();

      public void SendLowValueMessage() => NetworkDriver.SendLowValueMessage();

      public bool ReceiveLowValueMessage() => NetworkDriver.ReceiveLowValueMessage();
   }
}
