using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Shared.Events;
using PlantBasedPizza.Shared.Guards;

namespace PlantBasedPizza.Kitchen.Core.Handlers
{
    public class OrderSubmittedEventHandler : Handles<OrderSubmittedEvent>
    {
        private IKitchenRequestRepository _kitchenRequestRepository;
        private IRecipeService _recipeService;
        private readonly IOrderManagerService _orderManagerService;
        private ILogger<OrderSubmittedEventHandler> _logger;

        public OrderSubmittedEventHandler(IKitchenRequestRepository kitchenRequestRepository, IRecipeService recipeService, ILogger<OrderSubmittedEventHandler> logger, IOrderManagerService orderManagerService)
        {
            _kitchenRequestRepository = kitchenRequestRepository;
            _recipeService = recipeService;
            _logger = logger;
            _orderManagerService = orderManagerService;
        }
        
        public async Task Handle(OrderSubmittedEvent evt)
        {
            Guard.AgainstNull(evt, nameof(evt));

            this._logger.LogInformation("[KITCHEN] Logging order submitted event");

            var recipes = new List<RecipeAdapter>();
            
            var order = await this._orderManagerService.GetOrderDetails(evt.OrderIdentifier);
            
            this._logger.LogInformation($"[KITCHEN] Order has {order.Items.Count} item(s)");

            foreach (var recipe in order.Items)
            {
                this._logger.LogInformation($"[KITCHEN] Addig item {recipe.ItemName}");
                
                recipes.Add(await this._recipeService.GetRecipe(recipe.RecipeIdentifier));
            }

            var kitchenRequest = new KitchenRequest(evt.OrderIdentifier, recipes);

            this._logger.LogInformation("[KITCHEN] Storing kitchen request");

            await this._kitchenRequestRepository.AddNew(kitchenRequest);
        }
    }
}