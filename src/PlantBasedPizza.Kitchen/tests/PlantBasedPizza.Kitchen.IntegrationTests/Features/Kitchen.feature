Feature: Kitchen
	Kitchen request features

@NewOrderAppearsInList
Scenario: New order appears in list of kitchen new orders
	Given a new order submitted event is raised for order ORD8765
	Then order ORD8765 should appear in the kitchen new order list