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

    public async IAsyncEnumerable<PersistedEntity> ReadEntitiesAsync()
    {
        foreach (var entityFile in Directory.EnumerateFiles("Entities/", "*.json", SearchOption.TopDirectoryOnly))
        {
            logger.LogDebug("Reading entity file `{EntityFileName}`.", entityFile);
            await using FileStream stream = File.OpenRead(entityFile);
            var result = await JsonSerializer.DeserializeAsync<PersistedEntity[]>(stream, _jsonSerializerOptions);
            foreach (var entity in result!)
            {
                yield return entity;
            }
        }
    }

    public async IAsyncEnumerable<EntityArchetype> ReadArchetypesAsync()
    {
        var allArchetypeFiles = Directory.EnumerateFiles(
            "Entities/Archetypes/",
            "*.json",
            SearchOption.TopDirectoryOnly
        );

        foreach (var archetypeFile in allArchetypeFiles)
        {
            logger.LogDebug("Reading entity archetype file `{EntityArchetypeFileName}`.", archetypeFile);
            await using FileStream stream = File.OpenRead(archetypeFile);
            var result = await JsonSerializer.DeserializeAsync<EntityArchetype[]>(stream, _jsonSerializerOptions);
            foreach (var archetype in result!)
            {
                yield return archetype;
            }
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
