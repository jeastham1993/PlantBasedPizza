using System.Text.Json.Serialization;

namespace PlantBasedPizza.Shared.Logging;

public class ECSMetadata
{
    public string DockerId { get; set; }
    public string Name { get; set; }
    public string DockerName { get; set; }
    public string Image { get; set; }
    public string ImageID { get; set; }
    public Labels Labels { get; set; }
    public string DesiredStatus { get; set; }
    public string KnownStatus { get; set; }
    public Limits Limits { get; set; }
    public string CreatedAt { get; set; }
    public string StartedAt { get; set; }
    public string Type { get; set; }
    public string LogDriver { get; set; }
    public LogOptions LogOptions { get; set; }
    public string ContainerARN { get; set; }
    public Networks[] Networks { get; set; }
    public string Snapshotter { get; set; }
}

public class Labels
{
    [JsonPropertyName("com_amazonaws_ecs_cluster")]
    public string Cluster { get; set; }
    [JsonPropertyName("com_amazonaws_ecs_container_name")]
    public string ContainerName { get; set; }
    [JsonPropertyName("com_amazonaws_ecs_task_arn")]
    public string TaskArn { get; set; }
    [JsonPropertyName("com_amazonaws_ecs_task_definition_family")]
    public string TaskDefinitionFamily { get; set; }
    [JsonPropertyName("com_amazonaws_ecs_task_definition_version")]
    public string TaskDefinitionVersion { get; set; }
}

public class Limits
{
    public int CPU { get; set; }
}

public class LogOptions
{
    public string Host { get; set; }
    public string Name { get; set; }
    public string TLS { get; set; }
    public string dd_message_key { get; set; }
    public string dd_service { get; set; }
    public string dd_source { get; set; }
    public string dd_tags { get; set; }
    public string provider { get; set; }
}

public class Networks
{
    public string NetworkMode { get; set; }
    public string[] IPv4Addresses { get; set; }
    public int AttachmentIndex { get; set; }
    public string MACAddress { get; set; }
    public string IPv4SubnetCIDRBlock { get; set; }
    public string[] DomainNameServers { get; set; }
    public string[] DomainNameSearchList { get; set; }
    public string PrivateDNSName { get; set; }
    public string SubnetGatewayIpv4Address { get; set; }
}

