Feature: KitchenFeature
All features related to the kitchen module.

@kitchen
Scenario: Order should be moved through the requisite kitchen queues - prparing
	Given a new order is created with identifier KITCHEN5643
	When a CREATEORDERTEST is added to order KITCHEN5643
	And order KITCHEN5643 is submitted
	And order KITCHEN5643 is marked as preparing
	Then order KITCHEN5643 should appear in the preparing queue

@kitchen
Scenario: Order should be moved through the requisite kitchen queues - prep complete
	Given a new order is created with identifier KITCHEN7889
	When a CREATEORDERTEST is added to order KITCHEN7889
	And order KITCHEN7889 is submitted
	And order KITCHEN7889 is marked as preparing
	And order KITCHEN7889 is marked as prep-complete
	Then order KITCHEN7889 should appear in the baking queue

@kitchen
Scenario: Order should be moved through the requisite kitchen queues - bake complete
	Given a new order is created with identifier KITCHEN9006
	When a CREATEORDERTEST is added to order KITCHEN9006
	And order KITCHEN9006 is submitted
	And order KITCHEN9006 is marked as preparing
	And order KITCHEN9006 is marked as prep-complete
	And order KITCHEN9006 is marked as bake-complete
	Then order KITCHEN9006 should appear in the quality check queue