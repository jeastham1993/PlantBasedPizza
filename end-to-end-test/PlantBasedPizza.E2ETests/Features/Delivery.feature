Feature: DeliveryFeature
All features related to the delivery module.

@delivery
Scenario: WhenOrderDeliveredShouldMarkAsOrderComplete
	Given a new delivery order is created with identifier DELIVER7543 for customer james
	When a CREATEORDERTEST is added to order DELIVER7543
	And order DELIVER7543 is submitted
	And order DELIVER7543 is marked as preparing
	And order DELIVER7543 is marked as prep-complete
	And order DELIVER7543 is marked as bake-complete
	And order DELIVER7543 is marked as quality-checked
	And order DELIVER7543 is assigned to a driver named James
	And order DELIVER7543 is delivered
	Then order DELIVER7543 should no longer be assigned to a driver named James
	And order DELIVER7543 should contain a Order completed. event
	And the total points should be greater than 0 for james