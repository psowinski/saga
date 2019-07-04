using System.Threading.Tasks;
using Domain;

namespace Runner
{
   public interface ISaga
   {
      Task ProcessEvent(Event evn);
   }
}