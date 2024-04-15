Feature: KitchenFeature
All features related to the kitchen module.

@kitchen
Scenario: Order should be moved through the requisite kitchen queues - prparing
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