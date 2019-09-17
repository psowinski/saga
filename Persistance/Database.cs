using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
      private readonly List<JToken> data = new List<JToken>();

      public Task Save(string json)
      {
         var evn = JToken.Parse(json);
         lock (dataLock)
         {
            ValidateEvent(evn);
            StoreEvent(evn);
            AddByCategoryIndex(evn);
            return Task.CompletedTask;
         }
      }

      private void ValidateEvent(JToken evn)
      {
         var streamId = evn["streamId"].Value<string>();
         var version = evn["version"].Value<int>();
         ValidateStreamId(streamId);
         ValidateVersion(streamId, version);
      }

      private void StoreEvent(dynamic evn)
         => this.data.Add(evn);

      private void ValidateStreamId(string streamId)
      {
         if (string.IsNullOrEmpty(streamId) || !streamId.Contains('-'))
            throw new Exception("Invalid stream id");
      }
      private void ValidateVersion(string streamId, int version)
      {
         if (LastVersion(streamId) + 1 != version)
            throw new Exception("Invalid version");
      }

      private void AddByCategoryIndex(JToken evn)
      {
         var indexStreamId = ExtractByCategoryIndexStreamId(evn);
         var indexVersion = LastVersion(indexStreamId) + 1;
         var indexEvent = CreateByCategoryIndexEvent(indexStreamId, indexVersion, evn);
         this.data.Add(indexEvent);
      }

      private static string ExtractByCategoryIndexStreamId(JToken evn)
      {
         var eventStreamId = evn["streamId"].Value<string>();
         var indexStreamId = "byCategoryIndex-" + eventStreamId.Remove(eventStreamId.IndexOf('-'));
         return indexStreamId;
      }

      private JObject CreateByCategoryIndexEvent(string streamId, int version, JToken evn)
         => new JObject(
            new JProperty("type", "Indexed"),
            new JProperty("streamId", streamId),
            new JProperty("version", version),
            new JProperty("refStreamId", evn["streamId"]),
            new JProperty("refVersion", evn["version"]),
            new JProperty("refType", evn["type"]),
            new JProperty("refCorrelationId", evn["correlationId"]),
            new JProperty("refTimeStamp", evn["timeStamp"]));

      private int LastVersion(string streamId)
         => this.data.Where(x => x["streamId"].Value<string>() == streamId)
            .Select(x => x["version"].Value<int>())
            .DefaultIfEmpty(0)
            .Max();

      public Task<string> Load(string streamId, int fromVersion, int toVersion)
      {
         var events = this.data.Where(
            x => x["streamId"].Value<string>() == streamId
                 && x["version"].Value<int>() >= fromVersion
                 && (toVersion <= 0 || x["version"].Value<int>() <= toVersion));

         lock (dataLock)
         {
            var json = JsonConvert.SerializeObject(events, settings);
            return Task.FromResult(json);
         }
      }

      //Deprecated
      //public Task<int> FindLastVersion(string streamId)
      //{
      //   lock (dataLock)
      //   {
      //      return Task.FromResult(LastVersion(streamId));
      //   }
      //}

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
