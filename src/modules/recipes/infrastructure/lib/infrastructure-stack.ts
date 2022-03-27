import { aws_docdb, aws_ec2, CfnOutput, Stack, StackProps } from 'aws-cdk-lib';
import { SecurityGroup } from 'aws-cdk-lib/aws-ec2';
import { Construct } from 'constructs';
// import * as sqs from 'aws-cdk-lib/aws-sqs';

export class RecipeInfrastructureStack extends Stack {
  constructor(scope: Construct, id: string, props?: StackProps) {
    super(scope, id, props);

    const vpc = new aws_ec2.Vpc(this, 'TheVPC', {
      cidr: "10.0.0.0/16"
    });

    const lambdaSecurityGroup =  new aws_ec2.SecurityGroup(this, 'RecipeLambdaSG', {
      vpc: vpc,
      securityGroupName: 'recipes-lambda-sg'
    });

    const cluster = new aws_docdb.DatabaseCluster(this, 'RecipeDatabase', {
      masterUser: {
        username: 'recipes', // NOTE: 'admin' is reserved by DocumentDB
        excludeCharacters: '\"@/:', // optional, defaults to the set "\"@/" and is also used for eventually created rotations
        secretName: '/recipes/recipedb/masteruser', // optional, if you prefer to specify the secret name
      },
      instanceType: aws_ec2.InstanceType.of(aws_ec2.InstanceClass.T3, aws_ec2.InstanceSize.MEDIUM),
      vpcSubnets: {
        subnetType: aws_ec2.SubnetType.PRIVATE_WITH_NAT,
      },
      vpc,
    });
    cluster.addRotationSingleUser();

    const dbSecurityGroup = aws_ec2.SecurityGroup.fromSecurityGroupId(this, 'DbSg', cluster.securityGroupId);

    dbSecurityGroup.addIngressRule(aws_ec2.Peer.securityGroupId(lambdaSecurityGroup.securityGroupId), aws_ec2.Port.tcp(27017), 'Access from Lambda');
  }
}
