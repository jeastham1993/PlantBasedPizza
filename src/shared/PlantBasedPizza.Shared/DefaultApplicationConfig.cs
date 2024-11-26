namespace PlantBasedPizza.Shared;

public class DefaultApplicationConfig
{
    public string TeamName { get; set; } = "";
    public string ApplicationName { get; set; } = "";
    public string Version { get; set; } = "";
    public string Environment { get; set; } = "";
    public string DeployedAt { get; set; } = "";
    public string MemoryMb { get; set; } = "";
    public string CpuCount { get; set; } = "";
    public string CloudRegion { get; set; } = "";
}