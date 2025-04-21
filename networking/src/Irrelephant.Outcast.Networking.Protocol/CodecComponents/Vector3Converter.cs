using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Irrelephant.Outcast.Networking.Protocol.CodecComponents;

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
