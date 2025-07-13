
using System;
using System.Collections.Generic;
using Pulumi;
using Pulumi.Aws.Lambda;
using Pulumi.Aws.Iam;
using Pulumi.Aws.DynamoDB;
using Pulumi.Aws.DynamoDB.Inputs;

namespace webhook_nest.main.Infrastructure.Lambda.LambdaBuilder;

public class Lambda : ComponentResource
{
    private Pulumi.Aws.Iam.Role role;

    private string stage;
    private string name;
    private string lambdaPath;
    private Function lambdaFunction;
    private FunctionArgs args;
    private Table? dynamoDbTable;

    public Lambda(string name, string stage, string lambdaPath, FunctionArgs args)
        : base("custom:components:Lambda", $"webhooks-{name}")
    {

        this.name = name;
        this.lambdaPath = lambdaPath;
        this.stage = stage;
        this.args = args;
    }



    public Lambda Create()
    {


        var dependsOn = new List<Pulumi.Resource>();
        if (dynamoDbTable != null)
        {
            dependsOn.Add(dynamoDbTable);
        }

        lambdaFunction = new Function($"{stage}-{name}", new FunctionArgs
        {
            Runtime = args.Runtime,
            Handler = args.Handler,
            Role = args.Role,
            Code = new FileArchive(this.lambdaPath),
            Timeout = args.Timeout,
            MemorySize = args.MemorySize,
            Environment = args.Environment
        });

        return this;
    }



    public Function Build()
    {
        return lambdaFunction ?? throw new Exception("Unable to create lambda function");
    }
}