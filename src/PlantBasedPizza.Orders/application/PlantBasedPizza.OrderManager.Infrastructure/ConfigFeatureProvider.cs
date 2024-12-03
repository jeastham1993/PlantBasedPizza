using Microsoft.Extensions.Options;
using PlantBasedPizza.OrderManager.Core.Services;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class ConfigFeatureProvider(IOptions<Features> features) : IFeatures
{
    public bool UseOrchestrator()
    {
        return features.Value.UseOrchestrator;
    }
}