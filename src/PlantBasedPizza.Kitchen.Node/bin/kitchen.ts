import "source-map-support/register";
import * as cdk from "aws-cdk-lib";
import { KitchenStack } from "../lib/kitchen-stack";
import { IntegrationTestStack } from "../lib/integration-stack";
import { AwsSolutionsChecks } from 'cdk-nag';

const app = new cdk.App();

const version = process.env.VERSION ?? "latest";

const mStack = new KitchenStack(app, "KitchenStack", {
  env: {
    account: process.env["CDK_DEFAULT_ACCOUNT"],
    region: process.env["CDK_DEFAULT_REGION"],
  },
});

const integrationTestStack = new IntegrationTestStack(app, `KitchenTestStack-${version}`, {
  env: {
    account: process.env["CDK_DEFAULT_ACCOUNT"],
    region: process.env["CDK_DEFAULT_REGION"],
  },
});
