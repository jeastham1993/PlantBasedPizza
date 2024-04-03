Feature: LoyaltyPoints

@LoyaltyPoints
Scenario: Can take payment
	Then a payment is taken for 156.70 then the result should be successful