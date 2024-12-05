---
id: Payment Service
name: PaymentApi
version: 1.0.0
summary: ''
badges: []
sends:
  - id: paymentsuccessfuleventv1.message
    version: 1.0.0
  - id: paymentfailedeventv1.message
    version: 1.0.0
receives:
  - id: takepaymentcommand.message
    version: 1.0.0
  - id: refundpaymentcommand.message
    version: 1.0.0
schemaPath: payment-service.yml
specifications:
  asyncapiPath: payment-service.yml
---
## Architecture diagram
<NodeGraph />
