Feature: Orders
	OrdersFeature

@LoyaltyPointUpdatesAreCached
Scenario: LoyaltyPointsUpdatedAreCached
	Given a LoyaltyPointsUpdatedEvent is published for customer james, with a points total of 50.00
	Then loyalty points should be cached for james with a total amount of 50
	
@OrderWorkflow
Scenario: OrderCanBeCreated
    Given a new order is created
    When a CREATEORDERTEST is added to order
    And order is submitted
    Then order should contain a Submitted order. event
    
@CanProcessOrderPreparingEvent
Scenario: CanHandleOrderPreparingEvent
    Given a OrderPreparingEvent is published for customer james
    
@CanProcessOrderBakedEvent
Scenario: CanHandleOrderBakedEvent
    Given a OrderBakedEvent is published for customer james
    
@CanProcessOrderPrepCompleteEvent
Scenario: CanHandleOrderPrepCompleteEvent
    Given a OrderPrepCompleteEvent is published for customer james
    
@CanProcessOrderQualityCheckedEvent
Scenario: CanHandleOrderQualityCheckedEvent
    Given a OrderQualityCheckedEvent is published for customer james
    
@CanProcessDriverDeliveredEvent
Scenario: CanHandleDriverDeliveredEvent
    Given a DriverDeliveredOrderEvent is published for customer james
  
@CanProcessDriverCollectedEvent
Scenario: CanHandleDriverCollectedEvent
    Given a DriverCollectedOrderEvent is published for customer james