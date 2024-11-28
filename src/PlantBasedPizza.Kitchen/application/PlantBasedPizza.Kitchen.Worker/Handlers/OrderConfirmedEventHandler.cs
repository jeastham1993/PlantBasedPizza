using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.PublicEvents;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Kitchen.Worker.IntegrationEvents;
using PlantBasedPizza.Shared.Guards;

namespace PlantBasedPizza.Kitchen.Worker.Handlers
{
    public class OrderConfirmedEventHandler
    {
        private readonly IKitchenRequestRepository _kitchenRequestRepository;
        private readonly IRecipeService _recipeService;
        private readonly IOrderService _orderService;

        public OrderConfirmedEventHandler(IKitchenRequestRepository kitchenRequestRepository, IRecipeService recipeService, IOrderService orderService)
        {
            _kitchenRequestRepository = kitchenRequestRepository;
            _recipeService = recipeService;
            _orderService = orderService;
        }
        
        public async Task Handle(OrderConfirmedEventV1 evt)
        {
            Guard.AgainstNull(evt, nameof(evt));

            var recipes = new List<RecipeAdapter>();
            
            var orderDetails = await this._orderService.GetOrderDetails(evt.OrderIdentifier);

            foreach (var recipe in orderDetails.Items)
            {
                recipes.Add(await _recipeService.GetRecipe(recipe.RecipeIdentifier));
            }

            var kitchenRequest = new KitchenRequest(evt.OrderIdentifier, recipes);

            await _kitchenRequestRepository.AddNew(kitchenRequest, [new KitchenConfirmedOrderEventV1()
            {
                OrderIdentifier = evt.OrderIdentifier,
                KitchenIdentifier = kitchenRequest.KitchenRequestId
            }]);
        }
    }
}