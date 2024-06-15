﻿using System.Numerics;

namespace WebdiverGameFileTool.Util.MathExtras;

public static class MathExtrasExtensions {
    public static Matrix4x4 Normalize(this Matrix4x4 m) {
        if (m.M44 == 0)
            return default;
        for (var i = 0; i < 4; i++)
        for (var j = 0; j < 4; j++)
            m[i, j] /= m.M44;
        return m;
    }

    public static Vector3 DropW(this Vector4 value) => new(value.X, value.Y, value.Z);

    public static Vector3 ClampElements(in this Vector3 value, in Vector3 min, in Vector3 max) => new(
        float.Clamp(value.X, min.X, max.X),
        float.Clamp(value.Y, min.Y, max.Y),
        float.Clamp(value.Z, min.Z, max.Z));

    public static Vector3 TruncateElements(in this Vector3 value) =>
        new(
            value.X > 0f ? float.Floor(value.X) : float.Ceiling(value.X),
            value.Y > 0f ? float.Floor(value.Y) : float.Ceiling(value.Y),
            value.Z > 0f ? float.Floor(value.Z) : float.Ceiling(value.Z));

    public static Vector4 ClampElements(in this Vector4 value, in Vector4 min, in Vector4 max) => new(
        float.Clamp(value.X, min.X, max.X),
        float.Clamp(value.Y, min.Y, max.Y),
        float.Clamp(value.Z, min.Z, max.Z),
        float.Clamp(value.W, min.W, max.W));

    public static Vector4 TruncateElements(in this Vector4 value) =>
        new(
            value.X > 0f ? float.Floor(value.X) : float.Ceiling(value.X),
            value.Y > 0f ? float.Floor(value.Y) : float.Ceiling(value.Y),
            value.Z > 0f ? float.Floor(value.Z) : float.Ceiling(value.Z),
            value.W > 0f ? float.Floor(value.W) : float.Ceiling(value.W));

    public static bool AnyLessThan(in this Vector4 l, in Vector4 r) =>
        l.X < r.X || l.Y < r.Y || l.Z < r.Z || l.W < r.W;

    public static bool HasEquivalentValue(in this Vector3 l, in Vector3 r, float threshold) =>
        (l - r).LengthSquared() < threshold * threshold;

    public static bool HasEquivalentValue(in this Vector4 l, in Vector4 r, float threshold) =>
        (l - r).LengthSquared() < threshold * threshold;

    public static bool HasEquivalentValue(in this Quaternion l, in Quaternion r, float threshold) =>
        MathF.Abs(l.X - l.Y) <= threshold
        && MathF.Abs(l.Y - r.Y) <= threshold
        && MathF.Abs(l.Z - r.Z) <= threshold
        && MathF.Abs(l.W - r.W) <= threshold;

    public static bool IsIdentity(this Matrix4x4 m, double threshold) {
        if (MathF.Abs(m.M11 - 1) >= threshold) return false;
        if (MathF.Abs(m.M22 - 1) >= threshold) return false;
        if (MathF.Abs(m.M33 - 1) >= threshold) return false;
        if (MathF.Abs(m.M44 - 1) >= threshold) return false;
        if (MathF.Abs(m.M12) >= threshold) return false;
        if (MathF.Abs(m.M13) >= threshold) return false;
        if (MathF.Abs(m.M14) >= threshold) return false;
        if (MathF.Abs(m.M21) >= threshold) return false;
        if (MathF.Abs(m.M23) >= threshold) return false;
        if (MathF.Abs(m.M24) >= threshold) return false;
        if (MathF.Abs(m.M31) >= threshold) return false;
        if (MathF.Abs(m.M32) >= threshold) return false;
        if (MathF.Abs(m.M34) >= threshold) return false;
        if (MathF.Abs(m.M41) >= threshold) return false;
        if (MathF.Abs(m.M42) >= threshold) return false;
        if (MathF.Abs(m.M43) >= threshold) return false;
        return true;
    }

    public static bool IsZero(this Matrix4x4 m, double threshold) {
        if (MathF.Abs(m.M11) >= threshold) return false;
        if (MathF.Abs(m.M22) >= threshold) return false;
        if (MathF.Abs(m.M33) >= threshold) return false;
        if (MathF.Abs(m.M44) >= threshold) return false;
        if (MathF.Abs(m.M12) >= threshold) return false;
        if (MathF.Abs(m.M13) >= threshold) return false;
        if (MathF.Abs(m.M14) >= threshold) return false;
        if (MathF.Abs(m.M21) >= threshold) return false;
        if (MathF.Abs(m.M23) >= threshold) return false;
        if (MathF.Abs(m.M24) >= threshold) return false;
        if (MathF.Abs(m.M31) >= threshold) return false;
        if (MathF.Abs(m.M32) >= threshold) return false;
        if (MathF.Abs(m.M34) >= threshold) return false;
        if (MathF.Abs(m.M41) >= threshold) return false;
        if (MathF.Abs(m.M42) >= threshold) return false;
        if (MathF.Abs(m.M43) >= threshold) return false;
        return true;
    }

    public static List<float>? ToFloatList(this Vector3 val, Vector3 defaultValue, float threshold = 1e-6f) {
        if (Math.Abs(val.X - defaultValue.X) < threshold &&
            Math.Abs(val.Y - defaultValue.Y) < threshold &&
            Math.Abs(val.Z - defaultValue.Z) < threshold)
            return null;
        return new() {val.X, val.Y, val.Z};
    }

    public static List<float>? ToFloatList(this Quaternion val, Quaternion defaultValue, float threshold = 1e-6f) {
        if (Math.Abs(val.X - defaultValue.X) < threshold &&
            Math.Abs(val.Y - defaultValue.Y) < threshold &&
            Math.Abs(val.Z - defaultValue.Z) < threshold &&
            Math.Abs(val.W - defaultValue.W) < threshold)
            return null;
        return new() {val.X, val.Y, val.Z, val.W};
    }

    public static float GetComponent(this Quaternion q, int index) => index switch {
        0 => q.X,
        1 => q.Y,
        2 => q.Z,
        3 => q.W,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

    public static Vector3 DropW(this Quaternion q) => new(q.X, q.Y, q.Z);

    public static Vector3 ReadVector3(this BinaryReader r, FloatSize inputType = FloatSize.Single) =>
        inputType switch {
            FloatSize.Half => new((float) r.ReadHalf(), (float) r.ReadHalf(), (float) r.ReadHalf()),
            FloatSize.Single => new(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()),
            FloatSize.Double => new((float) r.ReadDouble(), (float) r.ReadDouble(), (float) r.ReadDouble()),
            _ => throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null),
        };

    public static Vector4 ReadVector4(this BinaryReader r, FloatSize inputType = FloatSize.Single) =>
        inputType switch {
            FloatSize.Half => new(
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf()),
            FloatSize.Single => new(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()),
            FloatSize.Double => new(
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble()),
            _ => throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null),
        };

    public static Vector4<byte> ReadVector4Byte(this BinaryReader r) =>
        new(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte());

    public static Quaternion ReadQuaternion(this BinaryReader r, FloatSize inputType = FloatSize.Single) =>
        inputType switch {
            FloatSize.Half => new(
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf()),
            FloatSize.Single => new(
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle()),
            FloatSize.Double => new(
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble()),
            _ => throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null),
        };

    public static AaBb ReadAaBb(this BinaryReader r, FloatSize inputType = FloatSize.Single) =>
        inputType switch {
            FloatSize.Half => new(
                new((float) r.ReadHalf(), (float) r.ReadHalf(), (float) r.ReadHalf()),
                new((float) r.ReadHalf(), (float) r.ReadHalf(), (float) r.ReadHalf())),
            FloatSize.Single => new(
                new(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()),
                new(r.ReadSingle(), r.ReadSingle(), r.ReadSingle())),
            FloatSize.Double => new(
                new((float) r.ReadDouble(), (float) r.ReadDouble(), (float) r.ReadDouble()),
                new((float) r.ReadDouble(), (float) r.ReadDouble(), (float) r.ReadDouble())),
            _ => throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null),
        };

    public static Matrix4x4 ReadMatrix4x4(this BinaryReader r, FloatSize inputType = FloatSize.Single) =>
        inputType switch {
            FloatSize.Half => new(
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf(),
                (float) r.ReadHalf()),
            FloatSize.Single => new(
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle(),
                r.ReadSingle()),
            FloatSize.Double => new(
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble(),
                (float) r.ReadDouble()),
            _ => throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null),
        };

    public static void Write(this BinaryWriter r, in Vector3 value, FloatSize inputType = FloatSize.Single) {
        switch (inputType) {
            case FloatSize.Half:
                r.Write((Half) value.X);
                r.Write((Half) value.Y);
                r.Write((Half) value.Z);
                break;
            case FloatSize.Single:
                r.Write(value.X);
                r.Write(value.Y);
                r.Write(value.Z);
                break;
            case FloatSize.Double:
                r.Write((double) value.X);
                r.Write((double) value.Y);
                r.Write((double) value.Z);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null);
        }
    }

    public static void Write(this BinaryWriter r, in Vector4 value, FloatSize inputType = FloatSize.Single) {
        switch (inputType) {
            case FloatSize.Half:
                r.Write((Half) value.X);
                r.Write((Half) value.Y);
                r.Write((Half) value.Z);
                r.Write((Half) value.W);
                break;
            case FloatSize.Single:
                r.Write(value.X);
                r.Write(value.Y);
                r.Write(value.Z);
                r.Write(value.W);
                break;
            case FloatSize.Double:
                r.Write((double) value.X);
                r.Write((double) value.Y);
                r.Write((double) value.Z);
                r.Write((double) value.W);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null);
        }
    }

    public static void Write(this BinaryWriter r, in Vector4<byte> value) {
        r.Write(value.X);
        r.Write(value.Y);
        r.Write(value.Z);
        r.Write(value.W);
    }

    public static void Write(this BinaryWriter r, in Quaternion value, FloatSize inputType = FloatSize.Single) {
        switch (inputType) {
            case FloatSize.Half:
                r.Write((Half) value.X);
                r.Write((Half) value.Y);
                r.Write((Half) value.Z);
                r.Write((Half) value.W);
                break;
            case FloatSize.Single:
                r.Write(value.X);
                r.Write(value.Y);
                r.Write(value.Z);
                r.Write(value.W);
                break;
            case FloatSize.Double:
                r.Write((double) value.X);
                r.Write((double) value.Y);
                r.Write((double) value.Z);
                r.Write((double) value.W);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(inputType), inputType, null);
        }
    }

    public static void Write(this BinaryWriter r, in AaBb value, FloatSize inputType = FloatSize.Single) {
        r.Write(value.Min, inputType);
        r.Write(value.Max, inputType);
    }

    public static void Write(this BinaryWriter r, in Matrix4x4 value, FloatSize inputType = FloatSize.Single) {
        r.Write(new Quaternion(value.M11, value.M12, value.M13, value.M14), inputType);
        r.Write(new Quaternion(value.M21, value.M22, value.M23, value.M24), inputType);
        r.Write(new Quaternion(value.M31, value.M32, value.M33, value.M34), inputType);
        r.Write(new Quaternion(value.M41, value.M42, value.M43, value.M44), inputType);
    }
}
