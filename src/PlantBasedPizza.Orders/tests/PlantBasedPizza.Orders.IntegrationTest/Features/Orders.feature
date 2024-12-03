Feature: Orders
	OrdersFeature
	
@OrderWorkflow
Scenario: Pickup order can be processed
    Given a new pickup order is created
    When a marg is added to order
    And order is submitted
	And user does not cancel 
    And payment is successful
	And kitchen quality checks the order
	And order is collected
    Then order should contain a Payment taken event
    And order should contain a Submitted order. event
	And order should contain a Order quality checked event
	And order should contain a Order awaiting collection event
	And order should contain a Order completed. event

@OrderWorkflow
Scenario: Delivery order can be processed
    Given a new delivery order is created
    When a marg is added to order
    And order is submitted
	And user does not cancel
    And payment is successful
	And kitchen quality checks the order
	And order is delivery successfully
    Then order should contain a Payment taken event
    And order should contain a Submitted order. event
	And order should contain a Order quality checked event
	And order should contain a Sending for delivery event
	And order should contain a Order completed. event
	
@OrderWorkflow
Scenario: Pickup order can be submitted and cancelled
    Given a new pickup order is created
    When a marg is added to order
    And order is submitted
    And order is cancelled
    Then order should not contain a Payment taken event
    And order should not contain a Submitted order. event

@OrderWorkflow
Scenario: A invalid payment success event is received
    Given an invalid payment success event is received
	Then message should arrive in dead letter inbox
	