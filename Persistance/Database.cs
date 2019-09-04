using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Infrastructure
{
   public class Database
   {
      private readonly JsonSerializerSettings settings = new JsonSerializerSettings()
      {
         MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
         Culture = CultureInfo.InvariantCulture
      };


      private readonly object dataLock = new object();
      private readonly List<dynamic> data = new List<dynamic>();
      public Task Save(string json)
      {
         lock (dataLock)
         {
            var evn = DeserializeEvent(json);

            UniqueIndex(evn);
            StoreEvent(evn);
            CategoryIndex(evn);
            return Task.CompletedTask;
         }
      }

      private dynamic DeserializeEvent(string json)
      {
         var evn = JsonConvert.DeserializeObject<dynamic>(json, this.settings);
         return evn;
      }

      private void StoreEvent(dynamic evn)
      {
         this.data.Add(evn);
      }

      private void UniqueIndex(dynamic evn)
      {
         (string streamId, int version) = (evn.StreamId, evn.Version);
         if (this.data.Any(x => x.StreamId == streamId && x.Version == version))
            throw new Exception("Version is not unique");
      }

      private void CategoryIndex(dynamic evn)
      {
         string eventStreamId = evn.StreamId;
         var streamId = "ByCategoryIndex-" + eventStreamId.Substring(0, eventStreamId.IndexOf('-'));
         var version = LastVersion(streamId) + 1;

         this.data.Add(CreateCategoryIndexEvent(streamId, version, evn));
      }

      private dynamic CreateCategoryIndexEvent(string streamId, int version, dynamic evn)
      {
         return DeserializeEvent("{" +
                                     "'$type': 'ByCategoryIndex'," +
                                    $"'StreamId': '{streamId}'," +
                                    $"'Version': {version}," +
                                    $"'RefStreamId': '{(string)evn.StreamId}'," +
                                    $"'RefVersion': {(int)evn.Version}," +
                                    $"'RefType': '{(string)evn["$type"]}'," +
                                    $"'RefCorrelationId': '{(string)evn.CorrelationId}'," +
                                    $"'RefTimeStamp': '{evn.TimeStamp.ToString("o", CultureInfo.InvariantCulture)}'" +
                                 "}");
      }

      private int LastVersion(string streamId)
      {
         return this.data.Where(x => x.StreamId == streamId)
                 .Select(x => (int)x.Version)
                 .DefaultIfEmpty(0)
                 .Max();
      }

      public Task<string> Load(string streamId, int fromVersion, int toVersion)
      {
         lock (dataLock)
         {
            var events = this.data.Where(
               x => x.StreamId == streamId 
                    && x.Version >= fromVersion
                    && (toVersion <= 0 || x.Version <= toVersion));

            var json = JsonConvert.SerializeObject(events, settings);
            return Task.FromResult(json);
         }
      }

      public Task<int> FindLastVersion(string streamId)
      {
         lock (dataLock)
         {
            return Task.FromResult(LastVersion(streamId));
         }
      }

      public string Dump()
      {
         lock (dataLock)
         {
            return JsonConvert.SerializeObject(this.data, new JsonSerializerSettings
            {
               Formatting = Formatting.Indented,
               MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
               Culture = CultureInfo.InvariantCulture
            });
         }
      }
   }
}
