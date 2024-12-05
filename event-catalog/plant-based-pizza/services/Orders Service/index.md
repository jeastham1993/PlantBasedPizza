---
id: Orders Service
name: OrdersService
version: 1.0.0
summary: ''
badges: []
sends:
  - id: ordersubmittedeventv1.message
    version: 1.0.0
  - id: ordercompletedintegrationeventv1.message
    version: 1.0.0
  - id: ordercompletedintegrationeventv2.message
    version: 1.0.0
  - id: orderreadyfordeliveryeventv1.message
    version: 1.0.0
  - id: orderconfirmedeventv1.message
    version: 1.0.0
  - id: ordercreatedeventv1.message
    version: 1.0.0
  - id: ordercancelledeventv1.message
    version: 1.0.0
receives:
  - id: drivercollectedordereventv1.message
    version: 1.0.0
  - id: driverdeliveredordereventv1.message
    version: 1.0.0
  - id: orderbakedeventv1.message
    version: 1.0.0
  - id: orderpreparingeventv1.message
    version: 1.0.0
  - id: orderprepcompleteeventv1.message
    version: 1.0.0
  - id: orderqualitycheckedeventv1.message
    version: 1.0.0
  - id: paymentsuccessfuleventv1.message
    version: 1.0.0
schemaPath: orders-service.yml
specifications:
  asyncapiPath: orders-service.yml
---
## Architecture diagram
<NodeGraph />
