using Godot;

namespace Irrelephant.Outcast.Client.Networking.Extensions;

public static class Vector3Extensions
{
    public static Vector3 ToGodotVector(this System.Numerics.Vector3 vector) =>
        new(vector.X, vector.Y, vector.Z);

    public static System.Numerics.Vector3 ToClrVector(this Vector3 vector) =>
        new(vector.X, vector.Y, vector.Z);
}
