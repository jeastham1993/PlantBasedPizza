using Dapr.AppCallback.Autogen.Grpc.v1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace PlantBasedPizza.LoyaltyPoints.Internal.Services;

public class AppCallbackService : AppCallback.AppCallbackBase
{
    public override async Task<ListTopicSubscriptionsResponse> ListTopicSubscriptions(Empty request, ServerCallContext context)
    {
        return new ListTopicSubscriptionsResponse();
    }

    public override async Task<ListInputBindingsResponse> ListInputBindings(Empty request, ServerCallContext context)
    {
        return new ListInputBindingsResponse();
    }
}