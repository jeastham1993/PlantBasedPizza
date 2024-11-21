using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Kitchen.Worker.IntegrationEvents;
using PlantBasedPizza.Shared.Guards;

namespace PlantBasedPizza.Kitchen.Worker.Handlers
{
    public class OrderSubmittedEventHandler
    {
        private readonly IKitchenEventPublisher _eventPublisher;
        private readonly IKitchenRequestRepository _kitchenRequestRepository;
        private readonly IRecipeService _recipeService;
        private readonly ILogger<OrderSubmittedEventHandler> _logger;

        public OrderSubmittedEventHandler(IKitchenRequestRepository kitchenRequestRepository, IRecipeService recipeService, ILogger<OrderSubmittedEventHandler> logger, IKitchenEventPublisher eventPublisher)
        {
            _kitchenRequestRepository = kitchenRequestRepository;
            _recipeService = recipeService;
            _logger = logger;
            _eventPublisher = eventPublisher;
        }
        
        public async Task Handle(OrderSubmittedEventV1 evt)
        {
            Guard.AgainstNull(evt, nameof(evt));

            _logger.LogInformation("[KITCHEN] Logging order submitted event");

            var recipes = new List<RecipeAdapter>();
            
            _logger.LogInformation("[KITCHEN] Order has {itemCount} item(s)", evt.Items.Count);

            foreach (var recipe in evt.Items)
            {
                _logger.LogInformation("[KITCHEN] Adding item {itemName}", recipe.ItemName);
                
                recipes.Add(await _recipeService.GetRecipe(recipe.RecipeIdentifier));
            }

            var kitchenRequest = new KitchenRequest(evt.OrderIdentifier, recipes);

            _logger.LogInformation("[KITCHEN] Storing kitchen request");

            await _kitchenRequestRepository.AddNew(kitchenRequest);
            await _eventPublisher.PublishKitchenConfirmedOrderEventV1(kitchenRequest);
        }
    }
}