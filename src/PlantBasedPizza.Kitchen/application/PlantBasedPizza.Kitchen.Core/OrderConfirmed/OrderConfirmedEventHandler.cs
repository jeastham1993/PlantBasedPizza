using System.Diagnostics;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Core.Entities;
using PlantBasedPizza.Kitchen.Core.PublicEvents;
using PlantBasedPizza.Kitchen.Core.Services;
using PlantBasedPizza.Shared.Guards;
using Saunter.Attributes;

namespace PlantBasedPizza.Kitchen.Core.OrderConfirmed
{
    public class OrderConfirmedEventHandler(
        IKitchenRequestRepository kitchenRequestRepository,
        IRecipeService recipeService,
        IOrderService orderService)
    {
        [Channel("order.orderConfirmed.v1")]
        [PublishOperation(typeof(OrderConfirmedEventV1), OperationId = nameof(OrderConfirmedEventV1))]
        public async Task Handle(OrderConfirmedEventV1 evt)
        {
            Guard.AgainstNull(evt, nameof(evt));

            var existingKitchenRequest = await kitchenRequestRepository.Retrieve(evt.OrderIdentifier);

            if (existingKitchenRequest is not null)
            {
                Activity.Current?.AddTag("order.exists", true);
                return;
            }

            var recipes = new List<RecipeAdapter>();
            
            var orderDetails = await orderService.GetOrderDetails(evt.OrderIdentifier);

            foreach (var recipe in orderDetails.Items)
            {
                recipes.Add(await recipeService.GetRecipe(recipe.RecipeIdentifier));
            }

            var kitchenRequest = new KitchenRequest(evt.OrderIdentifier, recipes);

            await kitchenRequestRepository.AddNew(kitchenRequest, [new KitchenConfirmedOrderEventV1()
            {
                OrderIdentifier = evt.OrderIdentifier,
                KitchenIdentifier = kitchenRequest.KitchenRequestId
            }]);
        }
    }
}