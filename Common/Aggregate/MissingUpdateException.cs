using System;

namespace Common.Aggregate
{
   public class MissingUpdateException : Exception
   {
      public MissingUpdateException(string msg) : base(msg) { }
   }
}
