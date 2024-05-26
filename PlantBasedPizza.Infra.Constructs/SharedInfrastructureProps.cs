using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.Events;

namespace PlantBasedPizza.Infra.Constructs;

public record SharedInfrastructureProps(IVpc Vpc, IEventBus Bus, IApplicationLoadBalancer? InternalAlb, string CommitHash, string Environment);