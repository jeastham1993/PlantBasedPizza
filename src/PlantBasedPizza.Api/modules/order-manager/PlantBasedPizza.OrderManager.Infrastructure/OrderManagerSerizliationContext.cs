using System.Text.Json.Serialization;
using PlantBasedPizza.OrderManager.Core;

namespace PlantBasedPizza.OrderManager.Infrastructure;

[JsonSerializable(typeof(Order))]
[JsonSerializable(typeof(OrderHistory))]
[JsonSerializable(typeof(OrderItem))]
[JsonSerializable(typeof(DeliveryDetails))]
public partial class OrderManagerSerializationContext : JsonSerializerContext
{
}