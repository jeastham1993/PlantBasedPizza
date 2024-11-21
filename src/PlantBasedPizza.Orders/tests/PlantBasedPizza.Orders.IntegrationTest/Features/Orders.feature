Feature: Orders
	OrdersFeature
	
@OrderWorkflow
Scenario: Order can be created
    Given a new order is created
    When a CREATEORDERTEST is added to order
    And order is submitted
    Then order should contain a Submitted order. event