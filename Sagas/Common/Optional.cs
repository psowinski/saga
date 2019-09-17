using System;

namespace Sagas.Common
{
   public abstract class Optional<T>
   {
      public abstract Optional<U> Bind<U>(Func<T, Optional<U>> mapper);
      public abstract Optional<U> Map<U>(Func<T, U> mapper);
   }

   public class Some<T> : Optional<T>
   {
      public Some(T value)
      {
         this.Value = value;
      }

      public T Value { get; }

      public override Optional<U> Bind<U>(Func<T, Optional<U>> mapper) => mapper(Value);
      public override Optional<U> Map<U>(Func<T, U> mapper) => new Some<U>(mapper(Value));
   }


   public class None<T> : Optional<T>
   {
      public override Optional<U> Bind<U>(Func<T, Optional<U>> mapper) => new None<U>();
      public override Optional<U> Map<U>(Func<T, U> mapper) => new None<U>();
   }
}