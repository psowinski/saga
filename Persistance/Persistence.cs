﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Infrastructure
{
   public class Persistence
   {
      private static readonly Database Db = new Database();
      private readonly JsonSerializerSettings settings;

      public static string Dump() => Db.Dump();

      public Persistence()
      {
         this.settings = new JsonSerializerSettings
         {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
            , Converters = new List<JsonConverter> { new EventConverter() }
         };
      }

      public async Task Save<T>(IEnumerable<T> events)
      {
         foreach (var evn in events)
            await Save(evn);
      }

      public Task Save<T>(T evn) => Db.Save(JsonConvert.SerializeObject(evn, this.settings));

      public Task<List<T>> Load<T>(string streamId) => Load<T>(streamId, 0);

      public Task<List<T>> Load<T>(string streamId, int fromVersion) => Load<T>(streamId, fromVersion, 0);

      public async Task<List<T>> Load<T>(string streamId, int fromVersion, int toVersion)
      {
         var json = await Db.Load(streamId, fromVersion, toVersion);
         var events = JsonConvert.DeserializeObject<List<T>>(json, this.settings);
         return events;
      }

      public async Task<T> LoadEvent<T>(string streamId, int version)
      {
         var events = await Load<T>(streamId, version, version);
         return events.FirstOrDefault();
      }

      public string GetCategoryIndexStreamId(string category) => "byCategoryIndex-" + category;

      public async Task<T> GetState<T>(string streamId) where T : State
      {
         var zero = (T)Activator.CreateInstance(typeof(T), streamId);
         var events = await Load<Event>(streamId);
         events.ForEach(zero.Apply);
         return zero;
      }
   }
}
