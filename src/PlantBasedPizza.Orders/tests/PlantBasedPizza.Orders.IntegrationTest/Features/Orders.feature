#Feature: Orders
#	OrdersFeature
	
#@OrderWorkflow
#Scenario: OrderCanBeCreated
#    Given a new order is created
#    When a CREATEORDERTEST is added to order
#    And order is submitted
#    Then order should contain a Submitted order. event
    
#@OrderWorkflow
#Scenario: OrderProcessEndToEndFlow
#    Given an order is created and submitted
#    When the payment is successful
#    Then order should contain a Payment successful event
#    When a OrderPreparingEvent is published
#    Then order should contain a Order prep started event
#    When a OrderPrepCompleteEvent is published
#    Then order should contain a Order prep complete event
#    When a OrderBakedEvent is published
#    Then order should contain a Order baked event
#    When a OrderQualityCheckedEvent is published
#    Then order should contain a Order quality checked event
#    And order should be awaiting collection
#    When order is collected
#    Then order should contain a Order completed. event