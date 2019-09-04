using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Common;
using Newtonsoft.Json.Serialization;

namespace Infrastructure
{
   public class EventsSerializationBinder : DefaultSerializationBinder
   {
      private readonly Dictionary<string, Type> nameToType;
      private readonly Dictionary<Type, string> typeToName;

      public EventsSerializationBinder()
      {
         var types = typeof(Event).Assembly.GetTypes()
               .Where(x => typeof(Event).IsAssignableFrom(x) && x != typeof(Event));

         this.nameToType = types.ToDictionary(t => t.FullName, t => t);
         this.typeToName = nameToType.ToDictionary(t => t.Value, t => t.Key);
      }

      public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
      {
         if (this.typeToName.TryGetValue(serializedType, out typeName))
            assemblyName = null;
         else
            base.BindToName(serializedType, out assemblyName, out typeName);
      }

      public override Type BindToType(string assemblyName, string typeName) =>
         this.nameToType.TryGetValue(typeName, out var type) 
            ? type 
            : base.BindToType(assemblyName, typeName);
   }
}