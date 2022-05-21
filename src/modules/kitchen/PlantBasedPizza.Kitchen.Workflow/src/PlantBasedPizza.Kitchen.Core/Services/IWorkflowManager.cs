using PlantBasedPizza.Kitchen.Core.Entities;

namespace PlantBasedPizza.Kitchen.Core.Services;

public interface IWorkflowManager
{
    Task StartWorkflowExecution(KitchenRequest request);
}