using System.Threading.Tasks;
using Domain.Common;

namespace Sagas.Common
{
   public interface ISagaAction
   {
      Task ProcessEvent(Event evn);
   }
}