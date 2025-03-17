using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.Events;

namespace PlantBasedPizza.Infra.Constructs;

public record SharedInfrastructureProps(IVpc Vpc, IEventBus Bus, IHttpApi? InternalApi, string ServiceName, string CommitHash, string Environment);