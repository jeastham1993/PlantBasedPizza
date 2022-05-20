namespace PlantBasedPizza.Kitchen.Api;

public class Settings
{
    public Settings(string queueUrl)
    {
        this.QueueUrl = queueUrl;
    }
    
    public string QueueUrl { get; set; }
}