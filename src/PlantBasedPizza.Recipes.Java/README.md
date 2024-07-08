# SpringBoot on Lambda

Example repository for running a SpringBoot API on AWS Lambda. This GitHub repo coincides with a [series on Youtube](https://www.youtube.com/watch?v=A1rYiHTy9Lg&list=PLCOG9xkUD90IDm9tcY-5nMK6X6g8SD-Sz) walking through how to build and deploy SpringBoot API's on Lambda.

## Deployment

To deploy this sample into your own AWS account two seperate steps are required. The first is to deploy the infrastructure, the second to deploy the application code.

### Infrastructure

The infrastructure is built using the AWS CDK. The deployed resources contain:

- A VPC
- 2 private, 2 public subnets
- 2 NAT Gateways
- Postgres RDS Instance
- Secrets Manager secret for credentials
- Lambda function for applying database changes

To deploy the infrastructure, run the following commands in order:

```
cd infrastructure/db-setup
mvn clean install
cd ../cdk
cdk deploy
```

This will compile the db-update Lambda function and then deploy the CDK infrastructure to your account. 3 values will output to the console that are required to deploy the application.

### Application

The application is deployed using AWS SAM. You can [install AWS SAM here](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html). To deploy the application run the below commands:

```
sam build
sam deploy --guided
```

