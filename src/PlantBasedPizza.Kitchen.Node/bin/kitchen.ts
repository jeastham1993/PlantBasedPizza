import "source-map-support/register";
import * as cdk from "aws-cdk-lib";
import { Aspects } from "aws-cdk-lib";
import { KitchenStack } from "../lib/kitchen-stack";

const app = new cdk.App();

const mStack = new KitchenStack(app, "KitchenStack", {
  env: {
    account: process.env["CDK_DEFAULT_ACCOUNT"],
    region: process.env["CDK_DEFAULT_REGION"],
  },
});
