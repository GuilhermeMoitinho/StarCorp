using System.Text.Json;
using System.Text.Json.Serialization;

namespace StarCorp.IntegrationTests;

internal static class TestJson
{
    public static readonly JsonSerializerOptions Options = Build();

    private static JsonSerializerOptions Build()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
