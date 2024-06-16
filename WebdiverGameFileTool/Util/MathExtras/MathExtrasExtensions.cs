using System.Numerics;
using System.Runtime.InteropServices;

namespace WebdiverGameFileTool.Util.MathExtras;

public static class MathExtrasExtensions {
    private const float FloatListNullThreshold = 1e-6f;

    public static Matrix4x4 Normalize(this Matrix4x4 m) {
        if (m.M44 == 0)
            return default;
        foreach (ref var v in MemoryMarshal.Cast<Matrix4x4, float>(MemoryMarshal.CreateSpan(ref m, 1)))
            v /= m.M44;
        return m;
    }

    public static List<float>? ToFloatList(
        this in Vector3 val,
        in Vector3 defaultValue,
        float threshold = FloatListNullThreshold) => ToFloatListImpl(val, defaultValue, threshold);

    public static List<float>? ToFloatList(
        this in Quaternion val,
        in Quaternion defaultValue,
        float threshold = FloatListNullThreshold) => ToFloatListImpl(val, defaultValue, threshold);

    public static Matrix4x4 InvertOrThrow(this in Matrix4x4 val) =>
        Matrix4x4.Invert(val, out var inverted) ? inverted : throw new InvalidOperationException();

    private static List<float>? ToFloatListImpl<T>(
        in T val,
        in T defaultValue,
        float threshold = FloatListNullThreshold)
        where T : struct {
        var vspan = MemoryMarshal.Cast<T, float>(MemoryMarshal.CreateReadOnlySpan(in val, 1));
        var dspan = MemoryMarshal.Cast<T, float>(MemoryMarshal.CreateReadOnlySpan(in defaultValue, 1));
        var i = 0;
        while (i < vspan.Length && MathF.Abs(vspan[i] - dspan[i]) < threshold)
            i++;
        if (i == vspan.Length)
            return null;
        var res = new List<float>(vspan.Length);
        res.AddRange(vspan);
        return res;
    }
}
