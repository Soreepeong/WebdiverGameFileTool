using System.Diagnostics;
using WebdiverGameFileTool.Util;

namespace WebdiverGameFileTool.FileFormats.KmpChunks;

public struct ChainingMotionKmpChunk {
    public const uint ChunkMagic = 0x636D0000;

    public int Count;
    public Inner[] Values;

    public ChainingMotionKmpChunk(ref ReadOnlySpan<byte> data) {
        if (data.ReadAndAdvance<uint>() != (ChunkMagic | 8))
            throw new InvalidDataException();

        // note: ChainingMotion?
        this.Count = data.ReadAndAdvance<int>();
        this.Values = new Inner[this.Count];
        for (var i = 0; i < this.Count; i++)
            this.Values[i] = new(ref data);
    }

    [DebuggerDisplay("{Duration}s, {Count}")]
    public struct Inner {
        public float Duration;
        public int Count;
        public Inner2[] Values;
            
        public Inner(ref ReadOnlySpan<byte> data) {
            this.Duration = data.ReadAndAdvance<float>();
            this.Count = data.ReadAndAdvance<int>();
            this.Values = data.ReadAndAdvance<Inner2>(this.Count);
        }

        [DebuggerDisplay("{AnimationIndex}, {Value2}, {Value3}, {Value4}, {Value5}, {Value6}, {Value7}, {Value8}, {Value9}, {ValueA}")]
        public struct Inner2 {
            public int AnimationIndex;
            public float Value2;
            public float Value3;
            public float Value4;
            public float Value5;
            public int Value6;
            public float Value7;
            public float Value8;
            public float Value9;
            public float ValueA;
        }
    }
}