using System;
using Pulumi;
using Pulumi.Aws.DynamoDB;
using System.Linq;

namespace webhook_nest.main.Infrastructure.RolesAndPolicies;

public class RolesAndPolicies : ComponentResource
{

    private Pulumi.Aws.Iam.Role role;
    private Pulumi.Aws.Iam.RolePolicyAttachment policyAttachment;
    private Pulumi.Aws.Iam.RolePolicy dynamoDbPolicy;
    private string stage;
    private string name;

    public RolesAndPolicies(string name, string stage)
    : base("custom:components:rolesandpolicies", $"webhooks-{name}")
    {
        this.name = name;
        this.stage = stage;
    }


    public RolesAndPolicies CreateRole()
    {
        role = new Pulumi.Aws.Iam.Role($"{stage}-{name}-role", new()
        {
            AssumeRolePolicy = @"{
        ""Version"": ""2012-10-17"",
        ""Statement"": [
            {
                ""Action"": ""sts:AssumeRole"",
                ""Principal"": { ""Service"": ""lambda.amazonaws.com"" },
                ""Effect"": ""Allow""
            }
        ]
    }"
        });

        return this;
    }

    public RolesAndPolicies AttachPolicies()
    {

        policyAttachment = new Pulumi.Aws.Iam.RolePolicyAttachment("lambda-basic-execution", new()
        {
            Role = role.Name,
            PolicyArn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
        });

        return this;
    }

    public RolesAndPolicies AttachDynamoDbAccess(Table[] tables)
    {
        var arns = tables.Select(t => t.Arn).ToArray();

        // Combine ARNs into a single Output<string[]>
        var resourceList = Output.All(arns).Apply(arns =>
        {
            var arnList = string.Join("\", \"", arns);
            return $"[\"{arnList}\"]";
        });


        var gsiArns = tables.Select(t => Output.Format($"{t.Arn}/index/*")).ToArray();
        var gsiResourceList = Output.All(gsiArns).Apply(gsiArns =>
        {
            var arnList = string.Join("\", \"", gsiArns);
            return $"[\"{arnList}\"]";
        });

        dynamoDbPolicy = new Pulumi.Aws.Iam.RolePolicy($"{stage}-{name}-lambda-dynamodb", new()
        {
            Role = role.Name,
            Policy = Output.All(resourceList, gsiResourceList).Apply(tuple =>
            {
                var tableResources = tuple[0];
                var gsiResources = tuple[1];
                return $@"{{
            ""Version"": ""2012-10-17"",
            ""Statement"": [
                {{
                    ""Effect"": ""Allow"",
                    ""Action"": [
                        ""dynamodb:GetItem"",
                        ""dynamodb:PutItem"",
                        ""dynamodb:UpdateItem"",
                        ""dynamodb:Query"",
                        ""dynamodb:DescribeTable"",
                        ""dynamodb:ListTables""
                    ],
                    ""Resource"": {tableResources}
                }},
                {{
                    ""Effect"": ""Allow"",
                    ""Action"": [
                        ""dynamodb:Query"",
                        ""dynamodb:GetItem""
                    ],
                    ""Resource"": {gsiResources}
                }}
            ]
        }}";
            })
        });

        return this;
    }

    public Pulumi.Aws.Iam.Role Build()
    {
        return role ?? throw new Exception($"Unable to create role {stage}-{name}  ");
    }

}