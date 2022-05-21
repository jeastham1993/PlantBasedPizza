using System.Threading.Tasks;
using PlantBasedPizza.Kitchen.Core.Adapters;

namespace PlantBasedPizza.Kitchen.Core.Services
{
    public interface IOrderManagerService
    {
        Task<OrderAdapter> GetOrderDetails(string orderIdentifier);
    }
}