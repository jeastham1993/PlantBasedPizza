Feature: DeliveryFeature
All features related to the delivery module.

@delivery
Scenario: WhenOrderDeliveredShouldMarkAsOrderComplete
	Given a new delivery order is created for customer james
	When a marg is added to order
	And order is submitted
	And order is processed by the kitchen
	And order is assigned to a driver named James
	And order is delivered
	Then order should no longer be assigned to a driver named James
	And order should contain a Order completed. event
	And the total points should be greater than 0 for user-account