Feature: AccountFeature
All features related to the account API.

@account
Scenario: AUserShouldBeAbleToRegisterAndThenLogin
	Given a user registers
	Then they should be able to successfully login
	
@account
Scenario: AUserShouldBeAbleToRegisterAndThenFailToLoginWithInvalidPassword
	Given a user registers
	Then they should not be able to login with an invalid password

@account
Scenario: UnregisteredEmailsCantLogin
	Given an un-registered email address
	Then they should not be able to login

@account
Scenario: InvalidEmailsCantRegister
	Given an invalid email address
	Then registration should fail

@account
Scenario: EmptyEmailsCantRegister
	Given an empty email address
	Then registration should fail

@account
Scenario: InvalidPasswordsCantRegister
	Given an invalid password
	Then registration should fail

@account
Scenario: EmptyPasswordsCantRegister
	Given an empty password
	Then registration should fail