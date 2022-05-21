namespace PlantBasedPizza.Kitchen.Api;

public class Settings
{
    public Settings(Dictionary<string, string> queueUrls)
    {
        this.QueueUrls = queueUrls;
    }
    
    public Dictionary<string, string> QueueUrls { get; set; }
}