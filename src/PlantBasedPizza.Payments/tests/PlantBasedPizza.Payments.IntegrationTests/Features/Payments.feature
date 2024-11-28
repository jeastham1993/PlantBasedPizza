Feature: LoyaltyPoints

@LoyaltyPoints
Scenario: Can handle order submitted event
	When an order submitted event is received
	Then the payment should be processed and cached