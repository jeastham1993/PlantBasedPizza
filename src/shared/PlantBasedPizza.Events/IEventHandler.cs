namespace PlantBasedPizza.Events;

public interface IEventHandler
{
    Task Handle(string evtPayload);
}