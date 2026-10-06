// Operators, plus the named method each one delegates to. The named versions exist
// because CA2225 asks for them and because Vector2.Add(a, b) is what shows up in a stack
// trace; the operator is what shows up in the physics integrator.

namespace GEngine.Core;

/// <content>Arithmetic and comparison operators.</content>
public readonly partial struct Vector2
{
    /// <summary>Adds two vectors component-wise.</summary>
    /// <param name="left">First vector.</param>
    /// <param name="right">Second vector.</param>
    /// <returns>The sum.</returns>
    public static Vector2 operator +(Vector2 left, Vector2 right) => Add(left, right);

    /// <summary>Subtracts the right vector from the left, component-wise.</summary>
    /// <param name="left">Vector to subtract from.</param>
    /// <param name="right">Vector to subtract.</param>
    /// <returns>The difference.</returns>
    public static Vector2 operator -(Vector2 left, Vector2 right) => Subtract(left, right);

    /// <summary>Reverses a vector.</summary>
    /// <param name="value">The vector to reverse.</param>
    /// <returns>The vector pointing the other way.</returns>
    public static Vector2 operator -(Vector2 value) => Negate(value);

    /// <summary>Scales a vector.</summary>
    /// <param name="value">The vector to scale.</param>
    /// <param name="scale">The factor.</param>
    /// <returns>The scaled vector.</returns>
    public static Vector2 operator *(Vector2 value, float scale) => Multiply(value, scale);

    /// <summary>Scales a vector, with the factor written first.</summary>
    /// <param name="scale">The factor.</param>
    /// <param name="value">The vector to scale.</param>
    /// <returns>The scaled vector.</returns>
    public static Vector2 operator *(float scale, Vector2 value) => Multiply(value, scale);

    /// <summary>Divides a vector by a scalar.</summary>
    /// <param name="value">The vector to divide.</param>
    /// <param name="divisor">The scalar to divide by; zero gives infinities, as it does for any float.</param>
    /// <returns>The divided vector.</returns>
    public static Vector2 operator /(Vector2 value, float divisor) => Divide(value, divisor);

    /// <summary>Exact equality.</summary>
    /// <param name="left">First vector.</param>
    /// <param name="right">Second vector.</param>
    /// <returns>True when both components match bit for bit.</returns>
    public static bool operator ==(Vector2 left, Vector2 right) => left.Equals(right);

    /// <summary>Exact inequality.</summary>
    /// <param name="left">First vector.</param>
    /// <param name="right">Second vector.</param>
    /// <returns>True when either component differs.</returns>
    public static bool operator !=(Vector2 left, Vector2 right) => !left.Equals(right);

    /// <summary>Adds two vectors component-wise.</summary>
    /// <param name="left">First vector.</param>
    /// <param name="right">Second vector.</param>
    /// <returns>The sum.</returns>
    public static Vector2 Add(Vector2 left, Vector2 right) => new(left.X + right.X, left.Y + right.Y);

    /// <summary>Subtracts the right vector from the left, component-wise.</summary>
    /// <param name="left">Vector to subtract from.</param>
    /// <param name="right">Vector to subtract.</param>
    /// <returns>The difference.</returns>
    public static Vector2 Subtract(Vector2 left, Vector2 right) => new(left.X - right.X, left.Y - right.Y);

    /// <summary>Reverses a vector.</summary>
    /// <param name="value">The vector to reverse.</param>
    /// <returns>The vector pointing the other way.</returns>
    public static Vector2 Negate(Vector2 value) => new(-value.X, -value.Y);

    /// <summary>Scales a vector.</summary>
    /// <param name="value">The vector to scale.</param>
    /// <param name="scale">The factor.</param>
    /// <returns>The scaled vector.</returns>
    public static Vector2 Multiply(Vector2 value, float scale) => new(value.X * scale, value.Y * scale);

    /// <summary>Divides a vector by a scalar.</summary>
    /// <param name="value">The vector to divide.</param>
    /// <param name="divisor">The scalar to divide by; zero gives infinities, as it does for any float.</param>
    /// <returns>The divided vector.</returns>
    public static Vector2 Divide(Vector2 value, float divisor) => new(value.X / divisor, value.Y / divisor);
}
