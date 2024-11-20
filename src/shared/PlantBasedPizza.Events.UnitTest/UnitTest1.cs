using System.Text.Json;

namespace PlantBasedPizza.Events.UnitTest;

public class UnitTest1
{
    [Fact]
    public void EventSerializationTest_ShouldIncludeProperties()
    {
        var evt = new SampleEvent()
        {
            ProductId = "aproductid"
        };
        
        var jsonResult = this.Serialize(evt);
        
        Assert.Equal("{\"EventName\":\"SampleEvent\",\"EventVersion\":\"v1\",\"Source\":\"http://localhost\",\"ProductId\":\"aproductid\"}", jsonResult);
    }

    private string Serialize(IntegrationEvent integrationEvent)
    {
        return JsonSerializer.Serialize(integrationEvent as object);
    }
}

public class SampleEvent : IntegrationEvent
{
    public override string EventName => "SampleEvent";
    public override string EventVersion => "v1";
    public override Uri Source => new Uri("http://localhost");

    public string ProductId { get; set; } = "";
}