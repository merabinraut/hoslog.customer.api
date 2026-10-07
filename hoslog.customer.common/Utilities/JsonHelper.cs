using System.Text.Json;
using System.Text.Json.Nodes;

namespace hoslog.customer.common.Utilities;

public static class JsonHelper
{
    public static string EncryptFields(
        string json,
        HashSet<string> fieldsToEncrypt,
        Func<string, string> encrypt
    )
    {
        var node = JsonNode.Parse(json)
            ?? throw new InvalidOperationException("Invalid Json.");

        TransformNode(
            node,
            fieldsToEncrypt,
            encrypt,
            convertNumericStringToNumber: false
        );

        return node.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = false
        });
    }

    public static string DecryptFields(
        string json,
        HashSet<string> fieldsToDecrypt,
        Func<string, string> decrypt,
        bool convertNumericStringToNumber = false
    )
    {
        var node = JsonNode.Parse(json)
            ?? throw new InvalidOperationException("Invalid Json.");

        TransformNode(
            node,
            fieldsToDecrypt,
            decrypt,
            convertNumericStringToNumber
        );

        return node.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = false
        });
    }

    private static void TransformNode(
        JsonNode node,
        HashSet<string> fieldsToTransform,
        Func<string, string> transform,
        bool convertNumericStringToNumber
    )
    {
        if (node is JsonObject obj)
        {
            foreach (var key in obj.Select(kv => kv.Key).ToList())
            {
                var value = obj[key];
                if (value is null) continue;

                if (fieldsToTransform.Contains(key))
                {
                    obj[key] = TransformValue(
                        value,
                        transform,
                        convertNumericStringToNumber
                    );
                }
                else
                {
                    TransformNode(
                        value,
                        fieldsToTransform,
                        transform,
                        convertNumericStringToNumber
                    );
                }
            }
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr)
            {
                if (item != null)
                {
                    TransformNode(
                        item,
                        fieldsToTransform,
                        transform,
                        convertNumericStringToNumber
                    );
                }
            }
        }
    }

    private static JsonNode? TransformValue(
        JsonNode value,
        Func<string, string> transform,
        bool convertNumericStringToNumber
    )
    {
        if (value is JsonValue jsonValue)
        {
            return TransformSingleValue(
                jsonValue,
                transform,
                convertNumericStringToNumber
            );
        }

        if (value is JsonArray array)
        {
            var newArray = new JsonArray();

            foreach (var item in array)
            {
                if (item is null)
                {
                    newArray.Add(null);
                    continue;
                }

                if (item is JsonValue itemValue)
                {
                    newArray.Add(
                        TransformSingleValue(
                            itemValue,
                            transform,
                            convertNumericStringToNumber
                        )
                    );
                }
                else
                {
                    newArray.Add(item.DeepClone());
                }
            }

            return newArray;
        }

        return value.DeepClone();
    }

    private static JsonNode? TransformSingleValue(
        JsonValue value,
        Func<string, string> transform,
        bool convertNumericStringToNumber
    )
    {
        var raw = value.ToString();
        var transformedValue = transform(raw);

        if (convertNumericStringToNumber &&
            long.TryParse(transformedValue, out var numericValue))
        {
            return JsonValue.Create(numericValue);
        }

        return JsonValue.Create(transformedValue);
    }
}