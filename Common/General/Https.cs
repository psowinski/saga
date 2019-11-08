using System.Net.Http;

namespace Common.General
{
   public static class Https
   {
      public static HttpClient CreateClient()
      {
         var client = new HttpClient(
#if DEBUG
            new HttpClientHandler()
            {
               ServerCertificateCustomValidationCallback = (arg1, arg2, arg3, arg4) => true
            }
#endif
         );
         return client;
      }
   }
}
