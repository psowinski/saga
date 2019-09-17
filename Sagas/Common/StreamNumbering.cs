using System;

namespace Sagas.Common
{
   public static class StreamNumbering
   {
      public static bool ReadableNames = false;

      private static readonly object NumberLock = new object();
      private static int lastNumber = 0;

      private static int NextNumber()
      {
         lock (NumberLock)
            return ++lastNumber;
      }

      public static string NewStreamId<T>() => NewId(typeof(T).Name.ToLower());
      public static string NewCorrelationId() => NewId("saga");

      private static string NewId(string name) => ReadableNames ? $"{name}-{NextNumber():D5}" : $"{name}-{Guid.NewGuid()}";
   }
}
