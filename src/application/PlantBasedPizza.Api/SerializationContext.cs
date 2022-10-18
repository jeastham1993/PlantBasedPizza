using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using PlantBasedPizza.OrderManager.Core.Entites;
using PlantBasedPizza.Kitchen.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using PlantBasedPizza.Kitchen.Core.Adapters;
using PlantBasedPizza.Kitchen.Infrastructure.DataTransfer;

namespace PlantBasedPizza.Api;

[JsonSerializable(typeof(KitchenRequestDTO))]
[JsonSerializable(typeof(List<KitchenRequestDTO>))]
[JsonSerializable(typeof(Task<IEnumerable<KitchenRequestDTO>>))]
[JsonSerializable(typeof(Order))]
[JsonSerializable(typeof(OrderHistory))]
[JsonSerializable(typeof(OrderItem))]
[JsonSerializable(typeof(DeliveryDetails))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
[JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
public partial class ApiSerializationContext : JsonSerializerContext
{
}       