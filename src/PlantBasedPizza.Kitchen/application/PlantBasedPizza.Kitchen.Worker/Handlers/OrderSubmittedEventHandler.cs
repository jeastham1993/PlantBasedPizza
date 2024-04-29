using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.IntegrationEvents;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Kitchen.Worker.IntegrationEvents;
using PlantBasedPizza.Shared.Guards;
using PlantBasedPizza.Shared.Logging;

namespace PlantBasedPizza.Kitchen.Worker.Handlers
{
    public class OrderSubmittedEventHandler
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IObservabilityService _logger;
        private readonly IKitchenRequestRepository _kitchenRequestRepository;
        private readonly IRecipeService _recipeService;

        public OrderSubmittedEventHandler(IKitchenRequestRepository kitchenRequestRepository, IRecipeService recipeService, IObservabilityService logger, IEventPublisher eventPublisher)
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
                this._logger.Info($"[KITCHEN] Addig item {recipe.ItemName}");

                var foundRecipe = await this._recipeService.GetRecipe(recipe.RecipeIdentifier);

                foreach (var ingredient in foundRecipe.Ingredients)
                {
                    this._logger.Info(ingredient.Name);
                }
                
                recipes.Add(foundRecipe);
            }

            var kitchenRequest = new KitchenRequest(evt.OrderIdentifier, recipes);

            this._logger.Info("[KITCHEN] Storing kitchen request");

            await this._kitchenRequestRepository.AddNew(kitchenRequest);
            await this._eventPublisher.Publish(new KitchenConfirmedOrderEventV1()
            {
                OrderIdentifier = kitchenRequest.OrderIdentifier,
                KitchenIdentifier = kitchenRequest.KitchenRequestId
            });
        }
    }
}