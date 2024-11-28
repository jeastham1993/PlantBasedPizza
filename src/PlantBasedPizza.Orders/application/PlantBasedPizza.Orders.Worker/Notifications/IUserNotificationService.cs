namespace PlantBasedPizza.Orders.Worker.Notifications;

public interface IUserNotificationService
{
    Task NotifyPaymentSuccess(string customerIdentifier, string orderIdentifier);
    
    Task NotifyOrderPreparing(string customerIdentifier, string orderIdentifier);
    
    Task NotifyOrderPrepComplete(string customerIdentifier, string orderIdentifier);
    
    Task NotifyOrderBakeComplete(string customerIdentifier, string orderIdentifier);
    
    Task NotifyOrderQualityCheckComplete(string customerIdentifier, string orderIdentifier);
    
    Task NotifyOrderDriverAssigned(string customerIdentifier, string orderIdentifier);
    
    Task NotifyReadyForCollection(string customerIdentifier, string orderIdentifier);
}