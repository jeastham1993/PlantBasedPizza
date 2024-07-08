using System.Text.Json.Serialization;

namespace PlantBasedPizza.Shared.Logging;

public class EcsMetadata
{
    public string? DockerId { get; set; }
    public string? Name { get; set; }
    public string? Image { get; set; }
    public Labels Labels { get; set; }
    public Limits Limits { get; set; }
    public string? CreatedAt { get; set; }
    public string? StartedAt { get; set; }
    public string? ContainerARN { get; set; }
}

public class Labels
{
    [JsonPropertyName("com_amazonaws_ecs_task_definition_family")]
    public string? TaskDefinitionFamily { get; set; }
    [JsonPropertyName("com_amazonaws_ecs_task_definition_version")]
    public string? TaskDefinitionVersion { get; set; }
}

public class Limits
{
    public int CPU { get; set; }
}