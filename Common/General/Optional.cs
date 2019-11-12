using System;

namespace Common.General
{
   public abstract class Optional<T>
   {
      public abstract Optional<U> Bind<U>(Func<T, Optional<U>> mapper);
      public abstract Optional<U> Map<U>(Func<T, U> mapper);
      public abstract U Match<U>(Func<T, U> some, Func<U> none);
      public abstract T ValueOr(Func<T> generateValue);
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
      public override U Match<U>(Func<T, U> some, Func<U> none) => some(Value);
      public override T ValueOr(Func<T> generateValue) => Value;
   }


   public class None<T> : Optional<T>
   {
      public override Optional<U> Bind<U>(Func<T, Optional<U>> mapper) => new None<U>();
      public override Optional<U> Map<U>(Func<T, U> mapper) => new None<U>();
      public override U Match<U>(Func<T, U> some, Func<U> none) => none();
      public override T ValueOr(Func<T> generateValue) => generateValue();
   }
}