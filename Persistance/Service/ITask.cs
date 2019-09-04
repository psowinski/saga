using System.Threading.Tasks;

namespace Infrastructure.Service
{
   public interface ITask
   {
      Task Run();
   }
}