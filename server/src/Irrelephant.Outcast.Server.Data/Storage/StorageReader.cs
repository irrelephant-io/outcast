using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Irrelephant.Outcast.Server.Data.Contract;

namespace Irrelephant.Outcast.Server.Data.Storage;

public class StorageReader
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Converters = { new Vector3Converter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static IEnumerable<Stream> GetResourceStreams(string resourceNamespace)
    {
        var assembly = typeof(StorageReader).Assembly;
        return assembly
            .GetManifestResourceNames()
            .Where(it => it.StartsWith(resourceNamespace))
            .OrderBy(it => it)
            .Select(resource => assembly.GetManifestResourceStream(resource))
            .Where(it => it is not null)!;
    }

    private async IAsyncEnumerable<TResource> ReadResources<TResource>(
        string resourceNamespace
    )
    {
        foreach (var entitiesFile in GetResourceStreams(resourceNamespace))
        {
            var result = await JsonSerializer.DeserializeAsync<TResource[]>(
                entitiesFile,
                _jsonSerializerOptions
            );
            foreach (var entity in result!)
            {
                yield return entity;
            }
        }
    }

    const string EntitiesNamespace = "Irrelephant.Outcast.Server.Data.Resources.Entities";
    public IAsyncEnumerable<PersistedEntity> ReadEntitiesAsync() =>
        ReadResources<PersistedEntity>(EntitiesNamespace);

    const string ArchetypesNamespace = "Irrelephant.Outcast.Server.Data.Resources.Archetypes";
    public IAsyncEnumerable<EntityArchetype> ReadArchetypesAsync() =>
        ReadResources<EntityArchetype>(ArchetypesNamespace);
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
