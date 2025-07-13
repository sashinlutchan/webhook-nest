using System;
using System.Collections.Generic;
using Pulumi;
using Pulumi.Aws.ApiGateway;
using Pulumi.Aws.Lambda;
using AwsApiGateway = Pulumi.AwsApiGateway;

namespace webhook_nest.main.ViewModels;



public class Routes
{
    public string Name { get; set; }
    public string Path { get; set; }
    public AwsApiGateway.Method Method { get; set; }
    public Function EventHandler { get; set; }
}