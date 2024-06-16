using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using WebdiverGameFileTool.Util;

namespace WebdiverGameFileTool.FileFormats.KmpChunks;

public struct ModelKmpChunk {
    public const uint ChunkMagic = 0x6B4D0000;

    public int VertexCount;
    public Vector3AndSkinIndex[] Vertices;

    public int NormalCount;
    public Vector3AndSkinIndex[] Normals;

    public int SkinCount;
    public Skin[] Skins;

    public int MeshCount;
    public Mesh[] Meshes;

    public ModelKmpChunk(ref ReadOnlySpan<byte> data) {
        if (data.ReadAndAdvance<uint>() != (ChunkMagic | 8))
            throw new InvalidDataException();

        this.VertexCount = data.ReadAndAdvance<int>();
        this.NormalCount = data.ReadAndAdvance<int>();
        this.SkinCount = data.ReadAndAdvance<int>();

        this.Vertices = data.ReadAndAdvance<Vector3AndSkinIndex>(this.VertexCount);
        this.Normals = data.ReadAndAdvance<Vector3AndSkinIndex>(this.NormalCount);
        this.Skins = data.ReadAndAdvance<Skin>(this.SkinCount);

        this.MeshCount = data.ReadAndAdvance<int>();
        this.Meshes = new Mesh[this.MeshCount];
        for (var i = 0; i < this.MeshCount; i++)
            this.Meshes[i] = new(ref data);
    }

    [DebuggerDisplay("{Value}, {SkinIndex}")]
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3AndSkinIndex {
        public Vector3 Value;
        public int SkinIndex;
    }

    [DebuggerDisplay("{BoneIndex}, {Value2}, {Value3}")]
    [StructLayout(LayoutKind.Sequential)]
    public struct Skin {
        public int BoneIndex;
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

        [DebuggerDisplay("{VertexIndex}, {NormalIndex}, {SkinIndex}, {Uv}")]
        [StructLayout(LayoutKind.Sequential)]
        public struct MeshVertex {
            public int VertexIndex;
            public int NormalIndex;
            public int SkinIndex;
            public Vector2 Uv;
        }

        [DebuggerDisplay("{Vertex1}, {Vertex2}, {Vertex3}")]
        [StructLayout(LayoutKind.Sequential)]
        public struct IndexSet {
            public short Vertex1;
            public short Vertex2;
            public short Vertex3;
        }
    }
}