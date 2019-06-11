using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orders;

namespace Runner
{
   public class Bus
   {
      private readonly Persistence persistence = new Persistence();
      private readonly BuyingSaga buyingSaga;

      public Bus()
      {
         buyingSaga = new BuyingSaga(this, new Persistence());
      }

      public Task Pipe(Event evn) =>
         Pipe(new[] {evn});

      public async Task Pipe(IEnumerable<Event> events)
      {
         await this.persistence.Save(events);
         await Process(events);
      }

      private Task Process(IEnumerable<Event> events) =>
         Task.WhenAll(events.Select(OnEvent).ToList());

      private Task OnEvent(Event evn) =>
         this.buyingSaga.OnEvent(evn);
   }
}
