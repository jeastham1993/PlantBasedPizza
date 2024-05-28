using BackgroundWorkers.IntegrationEvents;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Shared.Guards;
using PlantBasedPizza.Shared.Logging;

namespace BackgroundWorkers.Handlers
{
    public class OrderConfirmedEventHandler
    {
        private readonly IKitchenEventPublisher _eventPublisher;
        private readonly IKitchenRequestRepository _kitchenRequestRepository;
        private readonly IRecipeService _recipeService;

        public OrderConfirmedEventHandler(IKitchenRequestRepository kitchenRequestRepository, IRecipeService recipeService, IKitchenEventPublisher eventPublisher)
        {
            _kitchenRequestRepository = kitchenRequestRepository;
            _recipeService = recipeService;
            _eventPublisher = eventPublisher;
        }
        
        public async Task Handle(OrderConfirmedEventV1 evt)
        {
            Guard.AgainstNull(evt, nameof(evt));

            var recipes = new List<RecipeAdapter>();

            foreach (var recipe in evt.Items)
            {
                recipes.Add(await this._recipeService.GetRecipe(recipe.RecipeIdentifier));
            }

            var kitchenRequest = new KitchenRequest(evt.OrderIdentifier, recipes);

            await this._kitchenRequestRepository.AddNew(kitchenRequest);
            await this._eventPublisher.PublishKitchenConfirmedOrderEventV1(kitchenRequest);
        }
    }
}