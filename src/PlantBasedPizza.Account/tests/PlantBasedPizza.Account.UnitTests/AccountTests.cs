using Microsoft.Extensions.Options;

namespace PlantBasedPizza.Account.UnitTests;

public class AccountTests
{
    [Fact]
    public async Task CanRegisterNewUser_ShouldAllowRegisterWithValidDetails()
    {
        var testEmailAddress = "test@test.com";
        var testPassword = "Password!234";
        var testAccountId = "1234";
        
        var userRepo = A.Fake<IUserAccountRepository>();
        var jwtOptions = Options.Create(new JwtConfiguration()
        {
            Issuer = "https://test.plantbasedpizza.com",
            Audience = "https://test.plantbasedpizza.com",
            Key = "This is a sample secret key - please don't use in production environment.'"
        });

        A.CallTo(() => userRepo.CreateAccount(A<UserAccount>.Ignored)).Returns(
            Task.FromResult(new UserAccount()
            {
                AccountId = testAccountId, AccountType = AccountType.User, EmailAddress = testEmailAddress,
                Password = testPassword
            }));

        var userAccountService = new UserAccountService(userRepo, jwtOptions);

        var result = await userAccountService.Register(
            new RegisterUserCommand() { EmailAddress = testEmailAddress, Password = testPassword }, AccountType.User);

        result.AccountId.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task CanRegisterNewDriver_ShouldAllowRegisterWithValidDetails()
    {
        var testEmailAddress = "test@test.com";
        var testPassword = "Password!234";
        var testAccountId = "1234";
        
        var userRepo = A.Fake<IUserAccountRepository>();
        var jwtOptions = Options.Create(new JwtConfiguration()
        {
            Issuer = "https://test.plantbasedpizza.com",
            Audience = "https://test.plantbasedpizza.com",
            Key = "This is a sample secret key - please don't use in production environment.'"
        });

        A.CallTo(() => userRepo.CreateAccount(A<UserAccount>.Ignored)).Returns(
            Task.FromResult(new UserAccount()
            {
                AccountId = testAccountId, AccountType = AccountType.User, EmailAddress = testEmailAddress,
                Password = testPassword
            }));

        var userAccountService = new UserAccountService(userRepo, jwtOptions);

        var result = await userAccountService.Register(
            new RegisterUserCommand() { EmailAddress = testEmailAddress, Password = testPassword }, AccountType.Driver);

        result.AccountId.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task CanRegisterNewStaffAccount_ShouldAllowRegisterWithValidDetails()
    {
        var testEmailAddress = "test@plantbasedpizza.com";
        var testPassword = "Password!234";
        var testAccountId = "1234";
        
        var userRepo = A.Fake<IUserAccountRepository>();
        var jwtOptions = Options.Create(new JwtConfiguration()
        {
            Issuer = "https://test.plantbasedpizza.com",
            Audience = "https://test.plantbasedpizza.com",
            Key = "This is a sample secret key - please don't use in production environment.'"
        });

        A.CallTo(() => userRepo.CreateAccount(A<UserAccount>.Ignored)).Returns(
            Task.FromResult(new UserAccount()
            {
                AccountId = testAccountId, AccountType = AccountType.User, EmailAddress = testEmailAddress,
                Password = testPassword
            }));

        var userAccountService = new UserAccountService(userRepo, jwtOptions);

        var result = await userAccountService.Register(
            new RegisterUserCommand() { EmailAddress = testEmailAddress, Password = testPassword }, AccountType.Staff);

        result.AccountId.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task CanRegisterNewStaffAccountWithInvalidEmail_ShouldFailToCreate()
    {
        var testEmailAddress = "test@test.com";
        var testPassword = "Password!234";
        var testAccountId = "1234";
        
        var userRepo = A.Fake<IUserAccountRepository>();
        var jwtOptions = Options.Create(new JwtConfiguration()
        {
            Issuer = "https://test.plantbasedpizza.com",
            Audience = "https://test.plantbasedpizza.com",
            Key = "This is a sample secret key - please don't use in production environment.'"
        });

        A.CallTo(() => userRepo.CreateAccount(A<UserAccount>.Ignored)).Returns(
            Task.FromResult(new UserAccount()
            {
                AccountId = testAccountId, AccountType = AccountType.User, EmailAddress = testEmailAddress,
                Password = testPassword
            }));

        var userAccountService = new UserAccountService(userRepo, jwtOptions);

        var exception = await Assert.ThrowsAsync<InvalidUserException>(async () => await userAccountService.Register(
            new RegisterUserCommand() { EmailAddress = testEmailAddress, Password = testPassword }, AccountType.Staff));
        
        exception.Reason.Should().Be("Not a valid staff email");
    }
    
    [Fact]
    public async Task CanRegisterAccountWithInvalidEmail_ShouldFailToCreate()
    {
        var testEmailAddress = "winefiwfnwe";
        var testPassword = "Password!234";
        var testAccountId = "1234";
        
        var userRepo = A.Fake<IUserAccountRepository>();
        var jwtOptions = Options.Create(new JwtConfiguration()
        {
            Issuer = "https://test.plantbasedpizza.com",
            Audience = "https://test.plantbasedpizza.com",
            Key = "This is a sample secret key - please don't use in production environment.'"
        });

        A.CallTo(() => userRepo.CreateAccount(A<UserAccount>.Ignored)).Returns(
            Task.FromResult(new UserAccount()
            {
                AccountId = testAccountId, AccountType = AccountType.User, EmailAddress = testEmailAddress,
                Password = testPassword
            }));

        var userAccountService = new UserAccountService(userRepo, jwtOptions);

        var exception = await Assert.ThrowsAsync<InvalidUserException>(async () => await userAccountService.Register(
            new RegisterUserCommand() { EmailAddress = testEmailAddress, Password = testPassword }, AccountType.User));

        exception.Reason.Should().Be("Invalid email address");
    }
    
    [Fact]
    public async Task CanRegisterAccountWithInvalidPassword_ShouldFailToCreate()
    {
        var testEmailAddress = "test@test.com";
        var testPassword = "this does not meet password requirements";
        var testAccountId = "1234";
        
        var userRepo = A.Fake<IUserAccountRepository>();
        var jwtOptions = Options.Create(new JwtConfiguration()
        {
            Issuer = "https://test.plantbasedpizza.com",
            Audience = "https://test.plantbasedpizza.com",
            Key = "This is a sample secret key - please don't use in production environment.'"
        });

        A.CallTo(() => userRepo.CreateAccount(A<UserAccount>.Ignored)).Returns(
            Task.FromResult(new UserAccount()
            {
                AccountId = testAccountId, AccountType = AccountType.User, EmailAddress = testEmailAddress,
                Password = testPassword
            }));

        var userAccountService = new UserAccountService(userRepo, jwtOptions);

        var exception = await Assert.ThrowsAsync<InvalidUserException>(async () => await userAccountService.Register(
            new RegisterUserCommand() { EmailAddress = testEmailAddress, Password = testPassword }, AccountType.User));

        exception.Reason.Should().Be("Invalid password");
    }
    
    [Fact]
    public async Task OnSuccessfulLogin_ShouldReturnValidJwt()
    {
        var testEmailAddress = "test@test.com";
        var testPassword = "this does not meet password requirements";
        var testAccountId = "1234";
        
        var userRepo = A.Fake<IUserAccountRepository>();
        var jwtOptions = Options.Create(new JwtConfiguration()
        {
            Issuer = "https://test.plantbasedpizza.com",
            Audience = "https://test.plantbasedpizza.com",
            Key = "This is a sample secret key - please don't use in production environment.'"
        });

        A.CallTo(() => userRepo.ValidateCredentials(A<string>.Ignored, A<string>.Ignored)).Returns(
            Task.FromResult(new UserAccount()
            {
                AccountId = testAccountId, AccountType = AccountType.User, EmailAddress = testEmailAddress,
                Password = testPassword
            }));

        var userAccountService = new UserAccountService(userRepo, jwtOptions);

        var loginResult = await userAccountService.Login(new LoginCommand()
        {
            EmailAddress = testEmailAddress,
            Password = testPassword
        });

        loginResult.AuthToken.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task OnFailedLogin_ShouldNotReturnJwt()
    {
        var testEmailAddress = "test@test.com";
        var testPassword = "this does not meet password requirements";
        var testAccountId = "1234";
        
        var userRepo = A.Fake<IUserAccountRepository>();
        var jwtOptions = Options.Create(new JwtConfiguration()
        {
            Issuer = "https://test.plantbasedpizza.com",
            Audience = "https://test.plantbasedpizza.com",
            Key = "This is a sample secret key - please don't use in production environment.'"
        });

        A.CallTo(() => userRepo.ValidateCredentials(A<string>.Ignored, A<string>.Ignored))
            .Throws<LoginFailedException>();

        var userAccountService = new UserAccountService(userRepo, jwtOptions);

        await Assert.ThrowsAsync<LoginFailedException>(async () => await userAccountService.Login(new LoginCommand()
        {
            EmailAddress = testEmailAddress,
            Password = testPassword
        }));
    }
}