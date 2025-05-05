using System.Numerics;

namespace Irrelephant.Outcast.Server.Simulation.Space;

public static class Vector3Extensions
{
    /// <summary>
    /// Returns the unsigned minimum angle to the given vector, in radians.
    /// </summary>
    /// <param name="from">The first vector.</param>
    /// <param name="to">The other vector to compare this vector to.</param>
    /// <returns>The unsigned angle between the two vectors, in radians.</returns>
    public static double AngleTo(
        this Vector3 from,
        Vector3 to
    ) => Math.Atan2(Vector3.Cross(from, to).Length(), Vector3.Dot(from, to));

    /// <summary>
    /// Returns the signed angle to the given vector, in radians.
    /// The sign of the angle is positive in a counter-clockwise
    /// direction and negative in a clockwise direction when viewed
    /// from the side specified by the <paramref name="axis" />.
    /// </summary>
    /// <param name="from">The first vector.</param>
    /// <param name="to">The other vector to compare this vector to.</param>
    /// <param name="axis">The reference axis to use for the angle sign.</param>
    /// <returns>The signed angle between the two vectors, in radians.</returns>
    public static double SignedAngleTo(this Vector3 from, Vector3 to, Vector3 axis)
    {
        var v3 = Vector3.Cross(from, to);
        double num = Math.Atan2(v3.Length(), Vector3.Dot(from, to));
        return Vector3.Dot(v3, axis) >= 0.0 ? num : -num;
    }
}
