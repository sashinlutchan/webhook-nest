using System;
using System.Collections.Generic;
using Pulumi;
using Pulumi.Aws.Lambda;
using AwsApiGateway = Pulumi.AwsApiGateway;

namespace webhook_nest.main.Infrastructure.ApiGateway;

public class ApiGateway : ComponentResource
{
    private readonly List<AwsApiGateway.Inputs.RouteArgs> lambdaFunction;
    private AwsApiGateway.RestAPI api = null!;
    private string AppStage = null!;

    public Output<string> ApiUrl => api.Url;
    public Output<string> Arn => api.Url;

    public ApiGateway(List<AwsApiGateway.Inputs.RouteArgs> lambdaFunction, string stage)
        : base("custom:components:ApiGateway", $"webhook-api")
    {
        this.AppStage = stage;
        this.lambdaFunction = lambdaFunction;
    }

    public ApiGateway Create()
    {
        api = new AwsApiGateway.RestAPI("webhook-api", new()
        {
            Routes = lambdaFunction

        }, new ComponentResourceOptions
        {
            Parent = this,
            DependsOn = new List<Pulumi.Resource> { }
        });

        return this;
    }

    public ApiGateway Build()
    {
        return this;
    }
}