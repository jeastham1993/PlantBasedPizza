Feature: LoyaltyPoints

@CanAddLoyaltyPoints
Scenario: Can add loyalty points
	Given the loyalty points are added for customer James for order ORD123 with a value of 56.67
	Then the total points should be 57 for James