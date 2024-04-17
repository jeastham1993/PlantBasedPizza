Feature: OrderManager
Order manager feature


@orderManager
Scenario: When an order is collected it should be completed
	Given a new order is created for customer james-collect
	When a CREATEORDERTEST is added to order
	And order is submitted
	And order is processed by the kitchen
	And order is collected
	Then order should contain a Order completed. event
	And the total points should be greater than 0 for james-collect