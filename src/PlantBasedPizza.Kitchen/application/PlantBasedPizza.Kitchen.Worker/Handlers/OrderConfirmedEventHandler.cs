using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Kitchen.Worker.IntegrationEvents;
using PlantBasedPizza.Shared.Guards;

namespace PlantBasedPizza.Kitchen.Worker.Handlers
{
    public class OrderConfirmedEventHandler
    {
        private readonly IKitchenEventPublisher _eventPublisher;
        private readonly IKitchenRequestRepository _kitchenRequestRepository;
        private readonly IRecipeService _recipeService;
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderConfirmedEventHandler> _logger;

        public OrderConfirmedEventHandler(IKitchenRequestRepository kitchenRequestRepository, IRecipeService recipeService, ILogger<OrderConfirmedEventHandler> logger, IKitchenEventPublisher eventPublisher, IOrderService orderService)
        {
            _kitchenRequestRepository = kitchenRequestRepository;
            _recipeService = recipeService;
            _logger = logger;
            _eventPublisher = eventPublisher;
            _orderService = orderService;
        }
        
        public async Task Handle(OrderConfirmedEventV1 evt)
        {
            Guard.AgainstNull(evt, nameof(evt));

            _logger.LogInformation("[KITCHEN] Logging order submitted event");

            var recipes = new List<RecipeAdapter>();
            
            var orderDetails = await this._orderService.GetOrderDetails(evt.OrderIdentifier);
            
            _logger.LogInformation("[KITCHEN] Order has {itemCount} item(s)", orderDetails.Items.Count);

            foreach (var recipe in orderDetails.Items)
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