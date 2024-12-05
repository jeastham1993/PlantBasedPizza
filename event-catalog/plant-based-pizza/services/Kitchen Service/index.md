---
id: Kitchen Service
name: KitchenService
version: 1.0.0
summary: ''
badges: []
sends:
  - id: kitchenconfirmedordereventv1.message
    version: 1.0.0
  - id: orderbakedeventv1.message
    version: 1.0.0
  - id: orderpreparingeventv1.message
    version: 1.0.0
  - id: orderprepcompleteeventv1.message
    version: 1.0.0
  - id: orderqualitycheckedeventv1.message
    version: 1.0.0
receives:
  - id: orderconfirmedeventv1.message
    version: 1.0.0
schemaPath: kitchen-service.yml
specifications:
  asyncapiPath: kitchen-service.yml
---
## Architecture diagram
<NodeGraph />
