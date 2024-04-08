Feature: Orders
	OrdersFeature

@LoyaltyPointUpdatesAreCached
Scenario: Loyalty Point Updates are cached
	Given a LoyaltyPointsUpdatedEvent is published for customer james, with a points total of 50.00
	Then loyalty points should be cached for james with a total amount of 50