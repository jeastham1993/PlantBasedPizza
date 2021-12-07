Feature: OrderManager
Order manager feature

@orderManager
Scenario: Create a new order and add items
	Given a new order is created with identifier ORD1234
	When a CREATEORDERTEST is added to order ORD1234
	Then there should be 1 item on the order with identifier ORD1234

@orderManager
Scenario: Create a new order and submit then it should be added to the new kitchen requests
	Given a new order is created with identifier ORD5678
	When a CREATEORDERTEST is added to order ORD5678
		And order ORD5678 is submitted
	Then an order with identifier ORD5678 should be added to the new kitchen requests

@orderManager
Scenario: When an order is processed by the kitchen it should be marked complete in order management
	Given a new order is created with identifier ORD9786
	When a CREATEORDERTEST is added to order ORD9786
	And order ORD9786 is submitted
	And order ORD9786 is processed by the kitchen
	Then order ORD9786 should contain a Order quality checked event
	And order ORD9786 should be awaiting collection

@orderManager
Scenario: When an order is collected it should be completed
	Given a new order is created with identifier ORD6189
	When a CREATEORDERTEST is added to order ORD6189
	And order ORD6189 is submitted
	And order ORD6189 is processed by the kitchen
	And order ORD6189 is collected
	Then order ORD6189 should contain a Order completed. event