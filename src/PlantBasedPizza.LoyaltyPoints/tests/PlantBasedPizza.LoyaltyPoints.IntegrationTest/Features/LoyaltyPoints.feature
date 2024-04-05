Feature: LoyaltyPoints

@CanAddLoyaltyPoints
Scenario: Can add loyalty points
	Given the loyalty points are added for customer James for order ORD123 with a value of 56.67
	Then the total points should be 57 for James
	
@CanSpendLoyaltyPoints
Scenario: Can spend loyalty points
	Given the loyalty points are added for customer SpendyJames for order ORD229 with a value of 56.67
	When 20 points are spent for customer SpendyJames for order ORD789
	Then the total points should be 37 for SpendyJames