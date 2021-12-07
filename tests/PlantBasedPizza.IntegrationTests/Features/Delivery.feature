Feature: DeliveryFeature
All features related to the delivery module.

@delivery
Scenario: OrderShouldApppearReadyForDelivery
	Given a new delivery order is created with identifier DELIVER5643
	When a CREATEORDERTEST is added to order DELIVER5643
	And order DELIVER5643 is submitted
	And order DELIVER5643 is marked as preparing
	And order DELIVER5643 is marked as prep-complete
	And order DELIVER5643 is marked as bake-complete
	And order DELIVER5643 is marked as quality-checked
	Then order DELIVER5643 should be awaiting delivery collection

@delivery
Scenario: OrderCanHaveDriverAssigned
	Given a new delivery order is created with identifier DELIVER9876
	When a CREATEORDERTEST is added to order DELIVER9876
	And order DELIVER9876 is submitted
	And order DELIVER9876 is marked as preparing
	And order DELIVER9876 is marked as prep-complete
	And order DELIVER9876 is marked as bake-complete
	And order DELIVER9876 is marked as quality-checked
	And order DELIVER9876 is assigned to a driver named James
	Then order DELIVER9876 should appear in a list of James deliveries

@delivery
Scenario: WhenOrderDeliveredShouldMarkAsOrderComplete
	Given a new delivery order is created with identifier DELIVER7543
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