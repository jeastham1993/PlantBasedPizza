Feature: KitchenFeature
All features related to the kitchen module.

@kitchen
Scenario: Order appears as new
    Given a new order submitted event is raised
    Then order should appear as new
    
@kitchen
Scenario: Order appears as new multiple times, only should be stored once
    Given a new order submitted event is raised twice
    Then order should appear as new once

@kitchen
Scenario: Order should be moved through the requisite kitchen queues - preparing
    Given a new order submitted event is raised
    When order is marked as preparing
    Then order should appear in the preparing queue

@kitchen
Scenario: Order should be moved through the requisite kitchen queues - prep complete
    Given a new order submitted event is raised
    When order is marked as preparing
    And order is marked as prep-complete
    Then order should appear in the baking queue

@kitchen
Scenario: Order should be moved through the requisite kitchen queues - bake complete
    Given a new order submitted event is raised
    When order is marked as preparing
    And order is marked as prep-complete
    And order is marked as bake-complete
    Then order should appear in the quality check queue