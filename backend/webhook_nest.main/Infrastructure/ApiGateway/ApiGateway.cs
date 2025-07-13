using System;
using System.Collections.Generic;
using Pulumi;
using Pulumi.Aws.Lambda;
using webhook_nest.main.ViewModels;
using AwsApiGateway = Pulumi.AwsApiGateway;

namespace webhook_nest.main.Infrastructure.ApiGateway;

public class ApiGateway : ComponentResource
{
    private readonly List<Routes> lambdas;
    private AwsApiGateway.RestAPI api = null!;
    private string AppStage = null!;

    public Output<string> ApiUrl => api.Url;
    public Output<string> Arn => api.Url;

    public ApiGateway(List<Routes> lambdaFunctions, string stage)
        : base("custom:components:ApiGateway", $"webhook-api")
    {
        this.AppStage = stage;
        this.lambdas = lambdaFunctions;
    }

    public ApiGateway Create()
    {
        
        
        
        api = new AwsApiGateway.RestAPI("webhook-api", new()
        {
            Routes =
            {
                // GET /api/v1/webhook/getwebhook/{id}
                new AwsApiGateway.Inputs.RouteArgs
                {
                    Path = "/api/v1/webhook/getwebhook/{id}",
                    Method = AwsApiGateway.Method.GET,
                    EventHandler = lambdaFunction["webhook"]
                },
                // POST /api/v1/webhook/createwebhook
                new AwsApiGateway.Inputs.RouteArgs
                {
                    Path = "/api/v1/webhook/createwebhook",
                    Method = AwsApiGateway.Method.POST,
                    EventHandler = lambdaFunction["webhook"]
                },
                // POST /api/v1/webhook/updatewebhook/{id}
                new AwsApiGateway.Inputs.RouteArgs
                {
                    Path = "/api/v1/webhook/updatewebhook/{id}",
                    Method = AwsApiGateway.Method.POST,
                    EventHandler = lambdaFunction["webhook"]
                }
            }
        }, new ComponentResourceOptions
        {
            Parent = this,
            DependsOn = new List<Pulumi.Resource> { lambdaFunction }
        });

        return this;
    }

    public ApiGateway Build()
    {
        return this;
    }
}