using System;
using System.Collections.Generic;
using System.Linq;

namespace Sagas.Common
{
   public static class LinqChoose
   {
      public static IEnumerable<U> Choose<T, U>(this IEnumerable<T> source, Func<T, Optional<U>> selector)
         => source.Select(selector).OfType<Some<U>>().Select(x => x.Value);
   }
}