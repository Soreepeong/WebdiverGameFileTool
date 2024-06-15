using System.Diagnostics;
using System.Numerics;
using WebdiverGameFileTool.Util;

namespace WebdiverGameFileTool.FileFormats.KmpChunks;

public struct ModelKmpChunk {
    public const uint ChunkMagic = 0x6B4D0000;

    public int VertexCount;
    public Vector3AndInt[] Vertices;

    public int NormalCount;
    public Vector3AndInt[] Normals;

    public int Count3;
    public Something[] Data3;

    public int MeshCount;
    public Mesh[] Meshes;

    public ModelKmpChunk(ref ReadOnlySpan<byte> data) {
        if (data.ReadAndAdvance<uint>() != (ChunkMagic | 8))
            throw new InvalidDataException();

        this.VertexCount = data.ReadAndAdvance<int>();
        this.NormalCount = data.ReadAndAdvance<int>();
        this.Count3 = data.ReadAndAdvance<int>();

        this.Vertices = data.ReadAndAdvance<Vector3AndInt>(this.VertexCount);
        this.Normals = data.ReadAndAdvance<Vector3AndInt>(this.NormalCount);
        this.Data3 = data.ReadAndAdvance<Something>(this.Count3);

        this.MeshCount = data.ReadAndAdvance<int>();
        this.Meshes = new Mesh[this.MeshCount];
        for (var i = 0; i < this.MeshCount; i++)
            this.Meshes[i] = new(ref data);
    }

    [DebuggerDisplay("{Value1}, {Value2}")]
    public struct Vector3AndInt {
        public Vector3 Value1;
        public int Value2; // References to an item in Data3
    }

    [DebuggerDisplay("{Value1}, {Value2}, {Value3}")]
    public struct Something {
        public int Value1;
        public float Value2;
        public float Value3;
    }

    public struct Mesh {
        public int VertexCount;
        public int IndexCount;
        public float[] Unknown;
        public string FileName;
        public MeshVertex[] Vertices;
        public IndexSet[] Indices;

        public Mesh(ref ReadOnlySpan<byte> data) {
            this.VertexCount = data.ReadAndAdvance<int>();
            this.IndexCount = data.ReadAndAdvance<int>();
            this.Unknown = data.ReadAndAdvance<float>(0x11);
            this.FileName = data.ReadAndAdvanceString();
            this.Vertices = data.ReadAndAdvance<MeshVertex>(this.VertexCount);
            this.Indices = data.ReadAndAdvance<IndexSet>(this.IndexCount);
        }

        [DebuggerDisplay("{VertexIndex}, {NormalIndex}, {Data3Index}, {Uv}")]
        public struct MeshVertex {
            public int VertexIndex; // Index in Data1
            public int NormalIndex;
            public int Data3Index; // Index in Data3
            public Vector2 Uv;
        }

        [DebuggerDisplay("{Value1}, {Value2}, {Value3}")]
        public struct IndexSet {
            public short Value1;
            public short Value2;
            public short Value3;
        }
    }
}