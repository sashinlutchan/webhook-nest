using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json.Linq;

namespace webhook_nest.api.Shared;

public class Utils
{
      public static Dictionary<string, AttributeValue> ConvertToDynamoDBMap(Dictionary<string, object> data)
    {
        var map = new Dictionary<string, AttributeValue>();

        foreach (var kvp in data)
        {
            if (kvp.Value == null) continue;

            if (kvp.Value is string strValue)
            {
                if (!string.IsNullOrEmpty(strValue))
                    map[kvp.Key] = new AttributeValue { S = strValue };
            }
            else if (kvp.Value is int intValue)
            {
                map[kvp.Key] = new AttributeValue { N = intValue.ToString() };
            }
            else if (kvp.Value is double doubleValue)
            {
                map[kvp.Key] = new AttributeValue { N = doubleValue.ToString() };
            }
            else if (kvp.Value is bool boolValue)
            {
                map[kvp.Key] = new AttributeValue { BOOL = boolValue };
            }
            else if (kvp.Value is JObject jObject)
            {
                var jObjectDict = jObject.ToObject<Dictionary<string, object>>();
                if (jObjectDict != null)
                {
                    var nestedMap = ConvertToDynamoDBMap(jObjectDict);
                    if (nestedMap.Any())
                    {
                        map[kvp.Key] = new AttributeValue { M = nestedMap };
                    }
                }
            }
            else if (kvp.Value is JArray jArray)
            {
                var list = new List<AttributeValue>();
                foreach (var item in jArray)
                {
                    if (item.Type == JTokenType.String)
                        list.Add(new AttributeValue { S = item.ToString() });
                    else if (item.Type == JTokenType.Integer)
                        list.Add(new AttributeValue { N = item.ToString() });
                    else if (item.Type == JTokenType.Float)
                        list.Add(new AttributeValue { N = item.ToString() });
                    else if (item.Type == JTokenType.Boolean)
                        list.Add(new AttributeValue { BOOL = item.Value<bool>() });
                    else if (item.Type == JTokenType.Object)
                    {
                        var itemDict = item.ToObject<Dictionary<string, object>>();
                        if (itemDict != null)
                        {
                            var itemMap = ConvertToDynamoDBMap(itemDict);
                            if (itemMap.Any())
                            {
                                list.Add(new AttributeValue { M = itemMap });
                            }
                        }
                    }
                }
                if (list.Any())
                {
                    map[kvp.Key] = new AttributeValue { L = list };
                }
            }
            else
            {
                map[kvp.Key] = new AttributeValue { S = kvp.Value.ToString() };
            }
        }

        return map;
    }
      
    public static Dictionary<string, object> ConvertFromDynamoDBMap(Dictionary<string, AttributeValue> map)
    {
        var result = new Dictionary<string, object>();

        foreach (var kvp in map)
        {
            var attrValue = kvp.Value;

            if (attrValue.S != null)
            {
                result[kvp.Key] = attrValue.S;
            }
            else if (attrValue.N != null)
            {
                if (long.TryParse(attrValue.N, out var longValue))
                    result[kvp.Key] = longValue;
                else if (double.TryParse(attrValue.N, out var doubleValue))
                    result[kvp.Key] = doubleValue;
            }
            else if (attrValue.BOOL.HasValue)
            {
                result[kvp.Key] = attrValue.BOOL.Value;
            }
            else if (attrValue.M != null)
            {
                result[kvp.Key] = ConvertFromDynamoDBMap(attrValue.M);
            }
            else if (attrValue.L != null)
            {
                var list = new List<object>();
                foreach (var item in attrValue.L)
                {
                    if (item.S != null)
                        list.Add(item.S);
                    else if (item.N != null)
                    {
                        if (long.TryParse(item.N, out var longValue))
                            list.Add(longValue);
                        else if (double.TryParse(item.N, out var doubleValue))
                            list.Add(doubleValue);
                    }
                    else if (item.BOOL.HasValue)
                        list.Add(item.BOOL.Value);
                    else if (item.M != null)
                        list.Add(ConvertFromDynamoDBMap(item.M));
                }
                result[kvp.Key] = list;
            }
        }

        return result;
    }
      
}