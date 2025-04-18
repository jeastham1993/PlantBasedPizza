namespace PlantBasedPizza.Account.Core;

public class InvalidUserException : Exception
{
    public InvalidUserException(string reason)
    {
        this.Reason = reason;
    }
    
    public string Reason { get; set; }
}