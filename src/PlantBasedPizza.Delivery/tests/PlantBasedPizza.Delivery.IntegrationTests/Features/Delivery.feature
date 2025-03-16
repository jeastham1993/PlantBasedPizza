Feature: DeliveryFeature
All features related to the delivery module.

@delivery
Scenario: OrderShouldApppearReadyForDelivery
	Given an order is ready for delivery
	Then it should be awaiting delivery collection

@delivery
Scenario: OrderCanHaveDriverAssigned
	Given an order is ready for delivery
	When it is assigned to a driver named James
	Then it should appear in a list of James deliveries

@delivery
Scenario: WhenOrderDeliveredShouldMarkAsOrderComplete
	Given an order is ready for delivery
	When it is assigned to a driver named James
	And it is delivered
	Then it should no longer be assigned to a driver named James