using System;
using System.Collections.Generic;
using Pulumi;
using Pulumi.Aws.DynamoDB;
using Pulumi.Aws.DynamoDB.Inputs;
using Aws = Pulumi.Aws;

namespace webhook_nest.main.Infrastructure.Database;

public class DynamoDB  : ComponentResource
{
  

    private readonly string _name;
    private readonly string? _ttlAttributeName;
    private Table? _table;

    public DynamoDB(string name, string? ttlAttributeName = null)
        : base("custom:components:dynamodb", $"webhooks")
    {
        _name = name;
        _ttlAttributeName = ttlAttributeName;
    }

    public DynamoDB CreateTable()
    {
        _table = new Table(_name, new TableArgs
        {
            Name = _name,
            BillingMode = "PAY_PER_REQUEST",

            HashKey = "pk",
            RangeKey = "sk",
            GlobalSecondaryIndexes = new[]
            {
                new TableGlobalSecondaryIndexArgs
                {
                    Name = "LookUp",
                    HashKey = "GSI1PK",
                    RangeKey = "GSI1SK",
                    ProjectionType = "ALL"
                }
            },

            Attributes = new[]
            {
                new TableAttributeArgs
                {
                    Name = "pk",
                    Type = "S",
                },
                new TableAttributeArgs
                {
                    Name = "sk",
                    Type = "S",
                },
                new TableAttributeArgs
                {
                    Name = "GSI1PK",
                    Type = "S",
                },
                new TableAttributeArgs
                {
                    Name = "GSI1SK",
                    Type = "S",
                }
            },

            Ttl = !string.IsNullOrEmpty(_ttlAttributeName)
                ? new TableTtlArgs
                {
                    AttributeName = _ttlAttributeName,
                    Enabled = true
                }
                : null
        });

        return this;
    }

    public Table Build()
    {
        return _table ?? throw new InvalidOperationException("Call CreateTable() before Build().");
    }
}