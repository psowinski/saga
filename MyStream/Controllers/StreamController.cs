using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MyStream.Controllers
{
   [ApiController]
   [Route("[controller]")]
   public class StreamController : ControllerBase
   {
      private readonly Database db;
      private readonly ILogger<StreamController> logger;

      public StreamController(ILogger<StreamController> logger, Database db)
      {
         this.logger = logger;
         this.db = db;
      }

      [HttpGet("{streamId}")]
      public async Task<ActionResult<List<JsonElement>>> Get(string streamId)
      {
         return await db.Load(streamId);
      }

      [HttpGet("{streamId}/{from:int:min(1)}")]
      public async Task<ActionResult<List<JsonElement>>> Get(string streamId, int from)
      {
         return await db.Load(streamId, from);
      }

      [HttpGet("{streamId}/{from:int:min(1)}/{to:int:min(1)}")]
      public async Task<ActionResult<List<JsonElement>>> Get(string streamId, int from, int to)
      {
         return await db.Load(streamId, from, to);
      }

      [HttpGet("{streamId}/event/{version:int:min(1)}")]
      public async Task<ActionResult<JsonElement>> Event(string streamId, int version)
      {
         var events = await db.Load(streamId, version, version);
         if (events.Count == 1)
            return events[0];
         return NotFound();
      }

      [HttpPost]
      public async Task<ActionResult> Post([FromBody] JsonElement evn)
      {
         try
         {
            await db.Save(evn);
         }
         catch (Exception e)
         {
            this.logger.LogInformation(e.ToString());
            return BadRequest();
         }
         return Ok();
      }
   }
}
