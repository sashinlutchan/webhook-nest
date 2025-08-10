using System;
using System.Collections.Generic;
using System.Linq;
using Pulumi;
using Pulumi.Aws.ApiGateway;
using Pulumi.Aws.Lambda;
using AwsApiGateway = Pulumi.AwsApiGateway;

namespace webhook_nest.main.Infrastructure.ApiGateway;

public class ApiGateway : ComponentResource
{
    private readonly string AppStage;
    private RestApi restApi = null!;
    private Pulumi.Aws.ApiGateway.Deployment deployment = null!;
    private Stage stage = null!;

    public Output<string> ApiUrl => stage.InvokeUrl;
    public Output<string> Arn => restApi.Arn;
    public RestApi RestApi => restApi;

    public ApiGateway(string stage)
        : base("custom:components:ApiGateway", $"webhook-api")
    {
        this.AppStage = stage;
    }

    public ApiGateway Create()
    {
        restApi = new RestApi("webhook-gateway", new RestApiArgs
        {
            Name = "webhook-gateway",
            Description = "WebHook API Gateway"
        }, new CustomResourceOptions
        {
            Parent = this
        });

        var dummyMethod = new Method("dummy-method", new MethodArgs
        {
            RestApi = restApi.Id,
            ResourceId = restApi.RootResourceId,
            HttpMethod = "GET",
            Authorization = "NONE"
        }, new CustomResourceOptions { Parent = this });

        var dummyIntegration = new Integration("dummy-integration", new IntegrationArgs
        {
            RestApi = restApi.Id,
            ResourceId = restApi.RootResourceId,
            HttpMethod = dummyMethod.HttpMethod,
            Type = "MOCK",
            RequestTemplates = new Dictionary<string, string>
            {
                { "application/json", "{\"statusCode\": 200}" }
            }
        }, new CustomResourceOptions { Parent = this });

        var placeholderDeployment = new Pulumi.Aws.ApiGateway.Deployment("webhook-deployment-placeholder", new DeploymentArgs
        {
            RestApi = restApi.Id,
            Description = $"Placeholder deployment for {AppStage}"
        }, new CustomResourceOptions
        {
            Parent = this,
            DependsOn = { dummyMethod, dummyIntegration }
        });

        stage = new Stage("webhook-stage", new StageArgs
        {
            RestApi = restApi.Id,
            Deployment = placeholderDeployment.Id,
            StageName = AppStage
        }, new CustomResourceOptions { Parent = this });

        return this;
    }

    public void AddRoutes(List<AwsApiGateway.Inputs.RouteArgs> lambdaRoutes)
    {
        var resources = new Dictionary<string, Pulumi.Aws.ApiGateway.Resource>();
        var methods = new Dictionary<string, Method>();
        var integrations = new Dictionary<string, Integration>();
        var permissions = new List<Permission>();

        foreach (var route in lambdaRoutes)
        {
            var pathParts = route.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var currentPath = "";
            Pulumi.Aws.ApiGateway.Resource parentResource = null!;

            for (int i = 0; i < pathParts.Length; i++)
            {
                var part = pathParts[i];
                currentPath += "/" + part;

                if (!resources.ContainsKey(currentPath))
                {
                    var resourceName = $"resource-{currentPath.Replace("/", "-").Replace("{", "").Replace("}", "")}";

                    resources[currentPath] = new Pulumi.Aws.ApiGateway.Resource(resourceName, new Pulumi.Aws.ApiGateway.ResourceArgs
                    {
                        RestApi = restApi.Id,
                        ParentId = i == 0 ? restApi.RootResourceId : parentResource.Id,
                        PathPart = part
                    }, new CustomResourceOptions { Parent = this });
                }

                parentResource = resources[currentPath];
            }

            var finalResource = resources[route.Path.StartsWith("/") ? route.Path : "/" + route.Path];

            var methodKey = $"{route.Method}-{route.Path}";
            var methodName = $"method-{route.Method.ToString().ToLower()}-{route.Path.Replace("/", "-").Replace("{", "").Replace("}", "")}";

            methods[methodKey] = new Method(methodName, new MethodArgs
            {
                RestApi = restApi.Id,
                ResourceId = finalResource.Id,
                HttpMethod = route.Method.ToString(),
                Authorization = "NONE"
            }, new CustomResourceOptions { Parent = this });

            var integrationName = $"integration-{route.Method.ToString().ToLower()}-{route.Path.Replace("/", "-").Replace("{", "").Replace("}", "")}";
            integrations[methodKey] = new Integration(integrationName, new IntegrationArgs
            {
                RestApi = restApi.Id,
                ResourceId = finalResource.Id,
                HttpMethod = methods[methodKey].HttpMethod,
                IntegrationHttpMethod = "POST",
                Type = "AWS_PROXY",
                Uri = route.EventHandler.Apply(f => f.InvokeArn)
            }, new CustomResourceOptions { Parent = this });

            var methodResponseName = $"method-response-{route.Method.ToString().ToLower()}-{route.Path.Replace("/", "-").Replace("{", "").Replace("}", "")}";
            var methodResponse = new MethodResponse(methodResponseName, new MethodResponseArgs
            {
                RestApi = restApi.Id,
                ResourceId = finalResource.Id,
                HttpMethod = methods[methodKey].HttpMethod,
                StatusCode = "200",
                ResponseParameters = new Dictionary<string, bool>
                {
                    { "method.response.header.Access-Control-Allow-Origin", true },
                    { "method.response.header.Access-Control-Allow-Headers", true },
                    { "method.response.header.Access-Control-Allow-Methods", true },
                    { "method.response.header.Access-Control-Allow-Credentials", true }
                }
            }, new CustomResourceOptions { Parent = this });

            var integrationResponseName = $"integration-response-{route.Method.ToString().ToLower()}-{route.Path.Replace("/", "-").Replace("{", "").Replace("}", "")}";
            var integrationResponse = new IntegrationResponse(integrationResponseName, new IntegrationResponseArgs
            {
                RestApi = restApi.Id,
                ResourceId = finalResource.Id,
                HttpMethod = methods[methodKey].HttpMethod,
                StatusCode = methodResponse.StatusCode,
                ResponseParameters = new Dictionary<string, string>
                {
                    { "method.response.header.Access-Control-Allow-Origin", "'*'" },
                    { "method.response.header.Access-Control-Allow-Headers", "'Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token'" },
                    { "method.response.header.Access-Control-Allow-Methods", "'GET,POST,PUT,DELETE,OPTIONS'" },
                    { "method.response.header.Access-Control-Allow-Credentials", "'true'" }
                }
            }, new CustomResourceOptions { 
                Parent = this,
                DependsOn = { integrations[methodKey] }
            });

            // Add OPTIONS method for CORS preflight for this specific resource
            var optionsMethodName = $"method-options-{route.Path.Replace("/", "-").Replace("{", "").Replace("}", "")}";
            var optionsMethod = new Method(optionsMethodName, new MethodArgs
            {
                RestApi = restApi.Id,
                ResourceId = finalResource.Id,
                HttpMethod = "OPTIONS",
                Authorization = "NONE"
            }, new CustomResourceOptions { Parent = this });

            // Add CORS integration for OPTIONS method
            var optionsIntegrationName = $"integration-options-{route.Path.Replace("/", "-").Replace("{", "").Replace("}", "")}";
            var optionsIntegration = new Integration(optionsIntegrationName, new IntegrationArgs
            {
                RestApi = restApi.Id,
                ResourceId = finalResource.Id,
                HttpMethod = optionsMethod.HttpMethod,
                Type = "MOCK",
                RequestTemplates = new Dictionary<string, string>
                {
                    { "application/json", "{\"statusCode\": 200}" }
                }
            }, new CustomResourceOptions { Parent = this });

            // Add OPTIONS method response
            var optionsMethodResponseName = $"method-response-options-{route.Path.Replace("/", "-").Replace("{", "").Replace("}", "")}";
            var optionsMethodResponse = new MethodResponse(optionsMethodResponseName, new MethodResponseArgs
            {
                RestApi = restApi.Id,
                ResourceId = finalResource.Id,
                HttpMethod = optionsMethod.HttpMethod,
                StatusCode = "200",
                ResponseParameters = new Dictionary<string, bool>
                {
                    { "method.response.header.Access-Control-Allow-Headers", true },
                    { "method.response.header.Access-Control-Allow-Methods", true },
                    { "method.response.header.Access-Control-Allow-Origin", true },
                    { "method.response.header.Access-Control-Allow-Credentials", true }
                }
            }, new CustomResourceOptions { Parent = this });

            // Add OPTIONS integration response
            var optionsIntegrationResponseName = $"integration-response-options-{route.Path.Replace("/", "-").Replace("{", "").Replace("}", "")}";
            var optionsIntegrationResponse = new IntegrationResponse(optionsIntegrationResponseName, new IntegrationResponseArgs
            {
                RestApi = restApi.Id,
                ResourceId = finalResource.Id,
                HttpMethod = optionsMethod.HttpMethod,
                StatusCode = optionsMethodResponse.StatusCode,
                ResponseParameters = new Dictionary<string, string>
                {
                    { "method.response.header.Access-Control-Allow-Headers", "'Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token'" },
                    { "method.response.header.Access-Control-Allow-Methods", "'GET,POST,PUT,DELETE,OPTIONS'" },
                    { "method.response.header.Access-Control-Allow-Origin", "'*'" },
                    { "method.response.header.Access-Control-Allow-Credentials", "'true'" }
                }
            }, new CustomResourceOptions { 
                Parent = this,
                DependsOn = { optionsIntegration }
            });

            var permissionName = $"permission-{route.Method.ToString().ToLower()}-{route.Path.Replace("/", "-").Replace("{", "").Replace("}", "")}";
            permissions.Add(new Permission(permissionName, new PermissionArgs
            {
                Action = "lambda:InvokeFunction",
                Function = route.EventHandler.Apply(f => f.Name),
                Principal = "apigateway.amazonaws.com",
                SourceArn = restApi.ExecutionArn.Apply(arn => $"{arn}/*/*")
            }, new CustomResourceOptions { Parent = this }));
        }

        var rootOptionsMethod = new Method("root-options-method", new MethodArgs
        {
            RestApi = restApi.Id,
            ResourceId = restApi.RootResourceId,
            HttpMethod = "OPTIONS",
            Authorization = "NONE"
        }, new CustomResourceOptions { Parent = this });

        var rootCorsIntegration = new Integration("root-cors-integration", new IntegrationArgs
        {
            RestApi = restApi.Id,
            ResourceId = restApi.RootResourceId,
            HttpMethod = rootOptionsMethod.HttpMethod,
            Type = "MOCK",
            RequestTemplates = new Dictionary<string, string>
            {
                { "application/json", "{\"statusCode\": 200}" }
            }
        }, new CustomResourceOptions { Parent = this });


        var rootOptionsResponse = new MethodResponse("root-options-response", new MethodResponseArgs
        {
            RestApi = restApi.Id,
            ResourceId = restApi.RootResourceId,
            HttpMethod = rootOptionsMethod.HttpMethod,
            StatusCode = "200",
            ResponseParameters = new Dictionary<string, bool>
            {
                { "method.response.header.Access-Control-Allow-Headers", true },
                { "method.response.header.Access-Control-Allow-Methods", true },
                { "method.response.header.Access-Control-Allow-Origin", true },
                { "method.response.header.Access-Control-Allow-Credentials", true }
            }
        }, new CustomResourceOptions { Parent = this });


        var rootCorsIntegrationResponse = new IntegrationResponse("root-cors-integration-response", new IntegrationResponseArgs
        {
            RestApi = restApi.Id,
            ResourceId = restApi.RootResourceId,
            HttpMethod = rootOptionsMethod.HttpMethod,
            StatusCode = rootOptionsResponse.StatusCode,
            ResponseParameters = new Dictionary<string, string>
            {
                { "method.response.header.Access-Control-Allow-Headers", "'Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token'" },
                { "method.response.header.Access-Control-Allow-Methods", "'GET,POST,PUT,DELETE,OPTIONS'" },
                { "method.response.header.Access-Control-Allow-Origin", "'*'" },
                { "method.response.header.Access-Control-Allow-Credentials", "'true'" }
            }
        }, new CustomResourceOptions { 
            Parent = this,
            DependsOn = { rootCorsIntegration }
        });

        var allResources = new List<Pulumi.Resource>();
        allResources.AddRange(methods.Values.Cast<Pulumi.Resource>());
        allResources.AddRange(integrations.Values.Cast<Pulumi.Resource>());
        
        deployment = new Pulumi.Aws.ApiGateway.Deployment("webhook-deployment", new DeploymentArgs
        {
            RestApi = restApi.Id,
            Description = $"Deployment for {AppStage} with all routes"
        }, new CustomResourceOptions
        {
            Parent = this,
            DependsOn = allResources.ToArray()
        });


    }

    public ApiGateway Build()
    {
        return this;
    }
}