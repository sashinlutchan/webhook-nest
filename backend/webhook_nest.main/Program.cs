using Pulumi;
using Pulumi.Aws.S3;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pulumi.Aws;
using Pulumi.Aws.Lambda;
using Pulumi.Aws.Lambda.Inputs;
using webhook_nest.main.Infrastructure.Database;
using webhook_nest.main.Infrastructure.Lambda.LambdaBuilder;
using webhook_nest.main.Infrastructure.ApiGateway;
using webhook_nest.main.Infrastructure.RolesAndPolicies;
using Config = Pulumi.Config;
using aws = Pulumi.Aws;
using AwsApiGateway = Pulumi.AwsApiGateway;

return await Deployment.RunAsync(async () =>
{
    var config = new Config("app");

    var stage = config.Require("stage");
    var awsConfig = new Pulumi.Config("aws");
    var awsRegion = awsConfig.Require("region");

    var identity = await aws.GetCallerIdentity.InvokeAsync();
    var accountId = identity.AccountId;

    var table = new DynamoDB("WebHooks", "expiresAt")
        .CreateTable()
        .Build();

    var lambdaPath = webhook_nest.main.Infrastructure.Lambda.LambdaPackager.LambdaPackager.BuildAndZipLambda(
        lambdaProjectPath: "../webhook_nest.api/webhook_nest.api.csproj",
        outputFolder: "./bin/lambdas",
        zipName: "webhook-lambda.zip"
    );

    var lambdaRole = new RolesAndPolicies("lambdaRole", stage)
        .CreateRole()
        .AttachPolicies()
        .AttachDynamoDbAccess(new[] { table })
        .Build();


    var allowedOrigins = config.Get("allowedOrigins") ?? "*";

    var apiGateway = new ApiGateway(stage, allowedOrigins)
        .Create()
        .Build();

    var lambdaArgs = new FunctionArgs
    {
        Runtime = "dotnet8",
        Handler = "webhook_nest.api::webhook_nest.api.LambdaEntryPoint::FunctionHandlerAsync",
        Role = lambdaRole.Arn,
        Timeout = 60,
        MemorySize = 512,
        Environment = new FunctionEnvironmentArgs
        {
            Variables =
            {
                { "STAGE", stage },
                { "TABLE_NAME", table.Name },
                { "REGION", awsRegion },
                { "ALLOWED_ORIGINS", allowedOrigins }
            }
        }
    };


    List<AwsApiGateway.Inputs.RouteArgs> apis = new List<AwsApiGateway.Inputs.RouteArgs>()
    {
        new AwsApiGateway.Inputs.RouteArgs()
        {
            Method = AwsApiGateway.Method.POST,
            EventHandler = new Lambda("CreateWebHook", stage, lambdaPath, lambdaArgs)
                .Create()
                .Build(),
            Path = "/api/v1/webhook/createwebhook",
        },
        new AwsApiGateway.Inputs.RouteArgs()
        {
            Method = AwsApiGateway.Method.GET,
            EventHandler = new Lambda("GetWebHook", stage, lambdaPath, lambdaArgs)
                .Create()
                .Build(),
            Path = "/api/v1/webhook/getwebhook/{id}"
        },
        new AwsApiGateway.Inputs.RouteArgs()
        {
            Method = AwsApiGateway.Method.ANY,
            EventHandler = new Lambda("UpdateWebHook", stage, lambdaPath, lambdaArgs)
                .Create()
                .Build(),
            Path = "/api/v1/webhook/updatewebhook/{id}"
        },
        new AwsApiGateway.Inputs.RouteArgs()
        {
            Method = AwsApiGateway.Method.GET,
            EventHandler = new Lambda("GetEvents", stage, lambdaPath, lambdaArgs)
                .Create()
                .Build(),
            Path = "/api/v1/webhook/getwebhook/events/{id}"
        },
    };


    apiGateway.AddRoutes(apis);



    return new Dictionary<string, object?>
    {
        ["table"] = table.Id,
        ["lambdas"] = apis,
        ["apiUrl"] = apiGateway.ApiUrl,
    };
});