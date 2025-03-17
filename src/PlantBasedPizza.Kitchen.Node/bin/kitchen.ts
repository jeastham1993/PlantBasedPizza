import "source-map-support/register";
import * as cdk from "aws-cdk-lib";
import { KitchenStack } from "../lib/kitchen-stack";
import { AwsSolutionsChecks, NagSuppressions } from "cdk-nag";
const app = new cdk.App();

const environment = process.env.ENV ?? "test";
const serviceName = "KitchenService";
const version = process.env.VERSION ?? "latest";

cdk.Tags.of(app).add("plantbasedpizza:team", serviceName);
cdk.Tags.of(app).add("plantbasedpizza:owner", serviceName);
cdk.Tags.of(app).add("plantbasedpizza:business_unit", "Kitchen");

cdk.Tags.of(app).add("env", environment);
cdk.Tags.of(app).add("service", serviceName);
cdk.Tags.of(app).add("version", version);

const mStack = new KitchenStack(app, "KitchenStack", {
  stackName: `${serviceName}-${environment}`,
  env: {
    account: process.env["CDK_DEFAULT_ACCOUNT"],
    region: process.env["CDK_DEFAULT_REGION"],
  },
});
