using System.Threading.Tasks;
using Domain;

namespace Runner
{
   public interface ISagaAction
   {
      Task ProcessEvent(Event evn);
   }
}