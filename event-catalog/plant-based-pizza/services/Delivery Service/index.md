---
id: Delivery Service
name: DeliveryService
version: 1.0.0
summary: ''
badges: []
sends:
  - id: drivercollectedordereventv1.message
    version: 1.0.0
  - id: driverdeliveredordereventv1.message
    version: 1.0.0
receives:
  - id: orderreadyfordeliveryeventv1.message
    version: 1.0.0
schemaPath: delivery-service.yml
specifications:
  asyncapiPath: delivery-service.yml
---
## Architecture diagram
<NodeGraph />
