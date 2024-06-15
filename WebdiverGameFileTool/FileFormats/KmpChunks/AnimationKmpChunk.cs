using System.Diagnostics;
using System.Numerics;
using WebdiverGameFileTool.Util;

namespace WebdiverGameFileTool.FileFormats.KmpChunks;

public struct AnimationKmpChunk {
    public const uint ChunkMagic = 0x6B6D0000;

    public int Count;
    public Animation[] Animations;

    public AnimationKmpChunk(ref ReadOnlySpan<byte> data) {
        if (data.ReadAndAdvance<uint>() != (ChunkMagic | 8))
            throw new InvalidDataException();

        this.Count = data.ReadAndAdvance<int>();
        this.Animations = new Animation[this.Count];
        for (var i = 0; i < this.Count; i++)
            this.Animations[i] = new(ref data);
    }

    public struct Animation {
        public int AnimationCount;
        public float Duration;
        public float FrameDuration;
        public byte Byte1;
        public float Value3;
        public float Value4;
        public float Value5;

        public int SmthCount2;
        public Inner2[] Smth2;
            
        public int SmthCount3;
        public Inner2[] Smth3;
            
        public Track[] Tracks;
            
        public Animation(ref ReadOnlySpan<byte> data) {
            this.AnimationCount = data.ReadAndAdvance<int>();
            this.Duration = data.ReadAndAdvance<float>();
            this.FrameDuration = data.ReadAndAdvance<float>();
            this.Byte1 = data.ReadAndAdvance<byte>();
            this.Value3 = data.ReadAndAdvance<float>();
            this.Value4 = data.ReadAndAdvance<float>();
            this.Value5 = data.ReadAndAdvance<float>();

            this.SmthCount2 = data.ReadAndAdvance<int>();
            this.Smth2 = data.ReadAndAdvance<Inner2>(this.SmthCount2);

            this.SmthCount3 = data.ReadAndAdvance<int>();
            this.Smth3 = data.ReadAndAdvance<Inner2>(this.SmthCount3);

            this.Tracks = new Track[this.AnimationCount];
            for (var i = 0; i < this.AnimationCount; i++)
                this.Tracks[i] = new(ref data);
        }

        [DebuggerDisplay("{Value1}, {Value2}, {Value3}")]
        public struct Inner2 {
            public float Value1;
            public int Value2;
            public float Value3;
        }

        [DebuggerDisplay("{BoneIndex}, {Count}")]
        public struct Track {
            public int BoneIndex; // probably
            public int Count;
            public float[] Times;
            public TrsArray Trs;
                
            public Track(ref ReadOnlySpan<byte> data) { 
                this.BoneIndex = data.ReadAndAdvance<int>();
                this.Count = data.ReadAndAdvance<int>();
                this.Times = new float[this.Count];
                this.Trs = new(this.Count);
                for (var i = 0; i < this.Count; i++) {
                    this.Times[i] = data.ReadAndAdvance<float>();
                    this.Trs.Scale[i] = data.ReadAndAdvance<Vector3>();
                    this.Trs.Rotation[i] = data.ReadAndAdvance<Quaternion>();
                    this.Trs.Translation[i] = data.ReadAndAdvance<Vector3>();
                }
            }
        }
    }
}