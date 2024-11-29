namespace PlantBasedPizza.Kitchen.Worker;

public interface Idempotency
{
    Task<bool> HasEventBeenProcessedWithId(string eventId);
    
    Task ProcessedSuccessfully(string eventId);
}