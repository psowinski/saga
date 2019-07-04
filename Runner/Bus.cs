using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;

namespace Runner
{
   public class Bus
   {
      private readonly Persistence persistence = new Persistence();
      private readonly Network network = new Network();

      public Task Pipe(Event evn) => Pipe(new[] {evn});

      public async Task Pipe(IEnumerable<Event> events)
      {
         await this.persistence.Save(events);
         this.network.SendLowValueMessage();
      }
   }
}
