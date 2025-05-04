using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Irrelephant.Outcast.Server.Data.Contract;
using Irrelephant.Outcast.Server.Simulation.Components;
using Microsoft.Extensions.Logging;

namespace Irrelephant.Outcast.Server.Storage;

public class StorageReader(ILogger<StorageReader> logger)
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters = { new Vector3Converter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async IAsyncEnumerable<(GlobalId, Transform)> ReadEntitiesAsync(string regionName)
    {
        logger.LogDebug("Reading region {RegionName}.", regionName);

        await using FileStream stream = File.OpenRead($"Entities/{regionName}");
        var result = await JsonSerializer.DeserializeAsync<PersistedEntity[]>(stream, _jsonSerializerOptions);
        foreach (var entity in result!)
        {
            yield return
            (
                new GlobalId { Id = entity.Id },
                new Transform
                {
                    Position = entity.Transform.Position,
                    Rotation = entity.Transform.Rotation
                }
            );
        }
    }
}

public class Vector3Converter : JsonConverter<Vector3>
{
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            reader.Read();
        }
        else
        {
            throw new JsonException("Vector3 is serialised as an array with exactly three components.");
        }

        var x = reader.GetSingle();
        reader.Read();
        var y = reader.GetSingle();
        reader.Read();
        var z = reader.GetSingle();
        reader.Read();
        if (reader.TokenType == JsonTokenType.EndArray)
        {
            reader.Skip();
        }
        else
        {
            throw new JsonException("Vector3 is serialised as an array with exactly three components.");
        }

        return new Vector3(x, y, z);
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteNumberValue(value.Z);
        writer.WriteEndArray();
    }
}
