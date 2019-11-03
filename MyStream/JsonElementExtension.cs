using System.Text.Json;

namespace MyStream
{
   public static class JsonElementExtension
   {
      public static string GetStringPropertyValue(this JsonElement element, string name)
      {
         if (element.TryGetProperty(name, out var value))
            return value.GetString();
         return "";
      }
      public static long GetInt64PropertyValue(this JsonElement element, string name)
      {
         if (element.TryGetProperty(name, out var value))
            return value.GetInt64();
         return 0;
      }
   }
}
