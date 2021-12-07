Feature: KitchenFeature
All features related to the kitchen module.

@kitchen
Scenario: Order should be moved through the requisite kitchen queues - prparing
	Given a new order is created with identifier ORD5643
	When a CREATEORDERTEST is added to order ORD5643
	And order ORD5643 is submitted
	And order ORD5643 is marked as preparing
	Then order ORD5643 should appear in the preparing queue

@kitchen
Scenario: Order should be moved through the requisite kitchen queues - prep complete
	Given a new order is created with identifier ORD5643
	When a CREATEORDERTEST is added to order ORD5643
	And order ORD5643 is submitted
	And order ORD5643 is marked as preparing
	And order ORD5643 is marked as prep-complete
	Then order ORD5643 should appear in the baking queue

@kitchen
Scenario: Order should be moved through the requisite kitchen queues - bake complete
	Given a new order is created with identifier ORD5643
	When a CREATEORDERTEST is added to order ORD5643
	And order ORD5643 is submitted
	And order ORD5643 is marked as preparing
	And order ORD5643 is marked as prep-complete
	And order ORD5643 is marked as bake-complete
	Then order ORD5643 should appear in the quality check queue