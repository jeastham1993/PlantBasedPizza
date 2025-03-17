#!/usr/bin/env node
import "source-map-support/register";
import * as cdk from "aws-cdk-lib";
import { PlantBasedPizzaSharedInfrastructureStack } from "../lib/shared-infrastructure-stack"

const app = new cdk.App();

const sharedInfraStack = new PlantBasedPizzaSharedInfrastructureStack(app, "SharedInfraStack", {
    stackName: `SharedInfra-${process.env.ENV ?? "dev"}`
});