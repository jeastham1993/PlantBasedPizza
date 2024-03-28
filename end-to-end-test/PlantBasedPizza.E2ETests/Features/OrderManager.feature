Feature: OrderManager
Order manager feature


@orderManager
Scenario: When an order is collected it should be completed
	Given a new order is created with identifier ORD6189 for customer james-collect
	When a CREATEORDERTEST is added to order ORD6189
	And order ORD6189 is submitted
	And order ORD6189 is processed by the kitchen
	And order ORD6189 is collected
	Then order ORD6189 should contain a Order completed. event
	And the total points should be greater than 0 for james-collect