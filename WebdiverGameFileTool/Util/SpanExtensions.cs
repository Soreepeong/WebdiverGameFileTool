using System.Runtime.InteropServices;
using System.Text;

namespace WebdiverGameFileTool.Util;

public static unsafe class SpanExtensions {
    public static T ReadAndAdvance<T>(this ref ReadOnlySpan<byte> data) where T : unmanaged {
        var res = MemoryMarshal.Cast<byte, T>(data)[0];
        data = data[sizeof(T)..];
        return res;
    }

    public static T[] ReadAndAdvance<T>(this ref ReadOnlySpan<byte> data, int count) where T : unmanaged {
        var res = MemoryMarshal.Cast<byte, T>(data)[..count].ToArray();
        data = data[(sizeof(T) * count)..];
        return res;
    }

    public static string ReadAndAdvanceString(this ref ReadOnlySpan<byte> data) {
        var len = data.IndexOf((byte) 0);
        var res = Encoding.UTF8.GetString(data[..len]);
        data = data[(len + 1)..];
        return res;
    }
}
