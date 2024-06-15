using System.Diagnostics;
using System.Numerics;
using WebdiverGameFileTool.Util;

namespace WebdiverGameFileTool.FileFormats.KmpChunks;

public struct CollisionMeshKmpChunk {
    public const uint ChunkMagic = 0x6B430000;

    public int Count;
    public Inner[] Values;

    public CollisionMeshKmpChunk(ref ReadOnlySpan<byte> data) {
        if (data.ReadAndAdvance<uint>() != (ChunkMagic | 8))
            throw new InvalidDataException();

        this.Count = data.ReadAndAdvance<int>();
        this.Values = new Inner[this.Count];
        for (var i = 0; i < this.Count; i++)
            this.Values[i] = new(ref data);
    }

    public struct Inner {
        public int Count;
        public Inner2[] Values;
            
        public Inner(ref ReadOnlySpan<byte> data) {
            this.Count = data.ReadAndAdvance<int>();
            this.Values = new Inner2[this.Count];
            for (var i = 0; i < this.Count; i++)
                this.Values[i] = new(ref data);
        }

        [DebuggerDisplay("V={VertexCount,n}, I={IndexCount,n}, {Center}, {Value4}, {Value5}")]
        public struct Inner2 {
            public int VertexCount;
            public int IndexCount;
            public Vector3 Center;
            public float Value4;
            public int Value5;
            public Vector3[] Vertices;
            public IndexSetAndInt[] Indices;
                
            public Inner2(ref ReadOnlySpan<byte> data) {
                this.VertexCount = data.ReadAndAdvance<int>();
                this.IndexCount = data.ReadAndAdvance<int>();
                this.Center = data.ReadAndAdvance<Vector3>();
                this.Value4 = data.ReadAndAdvance<float>();
                this.Value5 = data.ReadAndAdvance<int>();
                this.Vertices = data.ReadAndAdvance<Vector3>(this.VertexCount);
                this.Indices = data.ReadAndAdvance<IndexSetAndInt>(this.IndexCount);
            }

            [DebuggerDisplay("{Value1}, {Value2}, {Value3}, {Value4}")]
            public struct IndexSetAndInt {
                public int Value1;
                public int Value2;
                public int Value3;
                public float Value4;
            }
        }
    }
}