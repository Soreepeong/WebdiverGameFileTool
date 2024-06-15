using System.Numerics;
using WebdiverGameFileTool.Util;

namespace WebdiverGameFileTool.FileFormats.KmpChunks;

public readonly struct TrsArray {
    public readonly Vector3[] Scale;
    public readonly Quaternion[] Rotation;
    public readonly Vector3[] Translation;

    public TrsArray(int count) {
        this.Scale = new Vector3[count];
        this.Rotation = new Quaternion[count];
        this.Translation = new Vector3[count];
    }

    public TrsArray(ref ReadOnlySpan<byte> data, int count) {
        this.Scale = data.ReadAndAdvance<Vector3>(count);
        this.Rotation = data.ReadAndAdvance<Quaternion>(count);
        this.Translation = data.ReadAndAdvance<Vector3>(count);
    }
}
