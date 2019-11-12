using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace MyStream
{
   public class Database
   {
      private readonly object dataLock = new object();
      private readonly List<JsonElement> data = new List<JsonElement>();

      public Task<List<JsonElement>> Load(string streamId)
         => Load(streamId, 0);

      public Task<List<JsonElement>> Load(string streamId, int fromVersion, int toVersion = -1)
      {
         lock (dataLock)
         {
            var events = this.data.Where(
                  x => x.GetStringPropertyValue("streamId") == streamId
                       && x.GetInt64PropertyValue("version") >= fromVersion
                       && (toVersion <= 0 || x.GetInt64PropertyValue("version") <= toVersion))
               .ToList();
            return Task.FromResult(events);
         }
      }

      public Task<List<string>> GetAllCategories()
      {
         lock (dataLock)
         {
            var categories = this.data
               .Where(
                  x => x.GetInt64PropertyValue("version") == 1 &&
                       x.GetStringPropertyValue("streamId").StartsWith("byCategoryIndex-"))
               .Select(x => x.GetStringPropertyValue("streamId").Substring(16))
               .ToList();
            return Task.FromResult(categories);
         }
      }

      public Task Save(JsonElement evn)
      {
         lock (dataLock)
         {
            SaveEventUnderLock(evn);
            return Task.CompletedTask;
         }
      }

      private void SaveEventUnderLock(JsonElement evn)
      {
         ValidateEvent(evn);
         StoreEvent(evn);
         AddByCategoryIndex(evn);
      }

      private void ValidateEvent(JsonElement evn)
      {
         var streamId = evn.GetStringPropertyValue("streamId");
         var version = evn.GetInt64PropertyValue("version");
         ValidateStreamId(streamId);
         ValidateVersion(streamId, version);
      }

      private void StoreEvent(JsonElement evn)
         => this.data.Add(evn);

      private void ValidateStreamId(string streamId)
      {
         if (string.IsNullOrEmpty(streamId) || !streamId.Contains('-'))
            throw new Exception("Invalid stream id");
      }
      private void ValidateVersion(string streamId, long version)
      {
         if (LastVersion(streamId) + 1 != version)
            throw new Exception("Invalid version");
      }

      private void AddByCategoryIndex(JsonElement evn)
      {
         var indexStreamId = ExtractByCategoryIndexStreamId(evn);
         var indexVersion = LastVersion(indexStreamId) + 1;
         var indexEvent = CreateByCategoryIndexEvent(indexStreamId, indexVersion, evn);
         this.data.Add(indexEvent);
      }

      private static string ExtractByCategoryIndexStreamId(JsonElement evn)
      {
         var eventStreamId = evn.GetStringPropertyValue("streamId");
         var indexStreamId = "byCategoryIndex-" + eventStreamId.Remove(eventStreamId.IndexOf('-'));
         return indexStreamId;
      }

      private JsonElement CreateByCategoryIndexEvent(string streamId, long version, JsonElement evn)
         => JsonDocument.Parse("{\"type\" : \"Indexed\"," +
            $"\"streamId\": \"{streamId}\"," +
            $"\"version\": {version}," +
            $"\"refStreamId\": \"{evn.GetStringPropertyValue("streamId")}\"," +
            $"\"refVersion\": {evn.GetInt64PropertyValue("version")}," +
            $"\"refType\": \"{evn.GetStringPropertyValue("type")}\"," +
            $"\"refCorrelationId\": \"{evn.GetStringPropertyValue("correlationId")}\"," +
            $"\"refTimeStamp\": \"{evn.GetStringPropertyValue("timeStamp")}\"}}").RootElement;

      private long LastVersion(string streamId)
         => this.data.Where(x => x.GetStringPropertyValue("streamId") == streamId)
            .Select(x => x.GetInt64PropertyValue("version"))
            .DefaultIfEmpty(0)
            .Max();
   }
}
