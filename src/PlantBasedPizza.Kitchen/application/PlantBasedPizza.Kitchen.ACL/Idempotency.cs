namespace PlantBasedPizza.Kitchen.ACL;

public interface Idempotency
{
    Task<bool> HasEventBeenProcessedWithId(string eventId);
    
    Task ProcessedSuccessfully(string eventId);
}