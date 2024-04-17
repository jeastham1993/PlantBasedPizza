namespace PlantBasedPizza.Account.Api.Core;

public enum AccountType
{
    User,
    Staff,
    Admin,
    Driver
}

public class UserAccount
{
    public string AccountId { get; set; }
    
    public string EmailAddress { get; set; }
    
    public string Password { get; set; }
    
    public AccountType AccountType { get; set; }

    public string AsAuthenticatedRole()
    {
        switch (this.AccountType)
        {
            case AccountType.Admin:
                return "admin";
            case AccountType.Staff:
                return "staff";
            case AccountType.Driver:
                return "driver";
            case AccountType.User:
                return "user";
        }

        return "user";
    }
}