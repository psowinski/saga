using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.General
{
   public static class LinqExt
   {
      public static IEnumerable<U> Choose<T, U>(this IEnumerable<T> source, Func<T, Optional<U>> selector)
         => source.Select(selector).OfType<Some<U>>().Select(x => x.Value);

      public static IEnumerable<TResult> LeftOuterJoin<TLeft, TRight, TKey, TResult>(this IEnumerable<TLeft> left, IEnumerable<TRight> right, Func<TLeft, TKey> leftKey, Func<TRight, TKey> rightKey,
         Func<TLeft, TRight, TResult> result)
      {
         return left.GroupJoin(right, leftKey, rightKey, (l, r) => new { l, r })
            .SelectMany(
               o => o.r.DefaultIfEmpty(),
               (l, r) => new { lft = l.l, rght = r })
            .Select(o => result.Invoke(o.lft, o.rght));
      }
   }
}