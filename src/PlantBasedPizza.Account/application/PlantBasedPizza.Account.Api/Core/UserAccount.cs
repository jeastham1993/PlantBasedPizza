using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PlantBasedPizza.Account.Api.Core;

public enum AccountType
{
    User,
    Staff,
    Admin,
    Driver
}

public enum AccountTier
{
    Std,
    Premium
}

public class UserAccount
{
    public static UserAccount Create(string emailAddress, string password, AccountType accountType)
    {
        if (!IsValidEmail(emailAddress))
        {
            throw new InvalidUserException("Invalid email address");
        }
        
        if (!IsValidPassword(password))
        {
            throw new InvalidUserException("Invalid password");
        }
        
        return new UserAccount()
        {
            AccountId = Guid.NewGuid().ToString(),
            EmailAddress = emailAddress,
            Password = HashPassword(password),
            AccountType = accountType,
            DateCreated = DateTime.UtcNow,
            AccountTier = AccountTier.Std
        };    
    }
    
    public string AccountId { get; set; } = string.Empty;
    
    public string EmailAddress { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;

    public int AccountAge => (DateTime.UtcNow - this.DateCreated).Days;
    
    public DateTime DateCreated { get; set; }
    
    public AccountTier AccountTier { get; set; }
    
    public AccountType AccountType { get; set; }

    public string AsAuthenticatedRole()
    {
        switch (AccountType)
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
    
    // Note: This hashing algorithm may not be suitable for production scenarios with real user data
    internal static string HashPassword(string password)
    {
        // Create a new instance of the SHA512 hash algorithm
        using SHA512 sha512Hash = SHA512.Create();
        // Convert the input string to a byte array and compute the hash
        byte[] data = sha512Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

        // Create a new Stringbuilder to collect the bytes
        // and create a string
        StringBuilder builder = new StringBuilder();

        // Loop through each byte of the hashed data 
        // and format each one as a hexadecimal string
        for (int i = 0; i < data.Length; i++)
        {
            builder.Append(data[i].ToString("x2"));
        }

        // Return the hashed string
        return builder.ToString();
    }
    
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Normalize the domain part of the email
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Check if the email is valid
            return Regex.IsMatch(email,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`{}|~\w])*)(?<=[0-9a-z])@))" + 
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
    
    private static bool IsValidPassword(string password)
    {
        // Password requirements
        var hasNumber = new Regex(@"[0-9]+");
        var hasUpperChar = new Regex(@"[A-Z]+");
        var hasLowerChar = new Regex(@"[a-z]+");
        var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

        // Validate the password
        return hasNumber.IsMatch(password) 
               && hasUpperChar.IsMatch(password) 
               && hasLowerChar.IsMatch(password) 
               && hasSymbols.IsMatch(password)
               && password.Length > 8;
    }
    
    private static string DomainMapper(Match match)
    {
        // Use IdnMapping class to convert Unicode domain names.
        var idn = new IdnMapping();

        // Pull out and process domain name (throws ArgumentException on invalid)
        var domainName = idn.GetAscii(match.Groups[2].Value);

        return match.Groups[1].Value + domainName;
    }
}