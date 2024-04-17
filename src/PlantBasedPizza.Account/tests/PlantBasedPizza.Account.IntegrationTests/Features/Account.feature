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