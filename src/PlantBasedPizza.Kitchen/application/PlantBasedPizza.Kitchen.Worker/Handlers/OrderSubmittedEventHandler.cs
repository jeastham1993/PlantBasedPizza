using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Kitchen.Infrastructure.IntegrationEvents;
using PlantBasedPizza.Kitchen.Worker.IntegrationEvents;
using PlantBasedPizza.Shared.Guards;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Kitchen.Worker.Handlers
{
    public class OrderSubmittedEventHandler
    {
        private readonly IKitchenEventPublisher _eventPublisher;
        private readonly IObservabilityService _logger;
        private readonly IKitchenRequestRepository _kitchenRequestRepository;
        private readonly IRecipeService _recipeService;

        public OrderSubmittedEventHandler(IKitchenRequestRepository kitchenRequestRepository, IRecipeService recipeService, IObservabilityService logger, IKitchenEventPublisher eventPublisher)
        {
            _kitchenRequestRepository = kitchenRequestRepository;
            _recipeService = recipeService;
            _logger = logger;
            _eventPublisher = eventPublisher;
        }
        
        public async Task Handle(OrderSubmittedEventV1 evt)
        {
            Guard.AgainstNull(evt, nameof(evt));

            this._logger.Info("[KITCHEN] Logging order submitted event");

            var recipes = new List<RecipeAdapter>();
            
            this._logger.Info($"[KITCHEN] Order has {evt.Items.Count} item(s)");

            foreach (var recipe in evt.Items)
            {
                this._logger.Info($"[KITCHEN] Adding item {recipe.ItemName}");
                
                recipes.Add(await this._recipeService.GetRecipe(recipe.RecipeIdentifier));
            }

            var kitchenRequest = new KitchenRequest(evt.OrderIdentifier, recipes);

            this._logger.Info("[KITCHEN] Storing kitchen request");

            await this._kitchenRequestRepository.AddNew(kitchenRequest);
            await this._eventPublisher.PublishKitchenConfirmedOrderEventV1(kitchenRequest);
        }
    }
}