using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace WebdiverGameFileTool;

public static unsafe class Program {
    public struct SkeletonKmpChunk {
        public const uint ChunkMagic = 0x6B530000;
        
        public int BoneCount;
        public int Unknown1;
        public int[] BoneParents;
        public Vector3[] Vec1;
        public Quaternion[] Quat1;
        public Vector3[] Vec2;
        public Vector3[] Vec3;
        public Quaternion[] Quat2;
        public Vector3[] Vec4;

        public SkeletonKmpChunk(ref ReadOnlySpan<byte> data) {
            if (data.ReadAndAdvance<uint>() != (ChunkMagic | 8))
                throw new InvalidDataException();
            this.BoneCount = data.ReadAndAdvance<int>();
            this.Unknown1 = data.ReadAndAdvance<int>();
            this.BoneParents = data.ReadAndAdvance<int>(this.BoneCount);
            this.Vec1 = data.ReadAndAdvance<Vector3>(this.BoneCount);
            this.Quat1 = data.ReadAndAdvance<Quaternion>(this.BoneCount);
            this.Vec2 = data.ReadAndAdvance<Vector3>(this.BoneCount);
            this.Vec3 = data.ReadAndAdvance<Vector3>(this.BoneCount);
            this.Quat2 = data.ReadAndAdvance<Quaternion>(this.BoneCount);
            this.Vec4 = data.ReadAndAdvance<Vector3>(this.BoneCount);
        }
    }

    public struct ModelKmpChunk {
        public const uint ChunkMagic = 0x6B4D0000;

        public int Count1;
        public Quaternion[] Quad1;

        public int Count2;
        public Quaternion[] Quad2;

        public int Count3;
        public Vector3[] Vec3;

        public int MeshCount;
        public Mesh[] Meshes;

        public ModelKmpChunk(ref ReadOnlySpan<byte> data) {
            if (data.ReadAndAdvance<uint>() != (ChunkMagic | 8))
                throw new InvalidDataException();

            this.Count1 = data.ReadAndAdvance<int>();
            this.Count2 = data.ReadAndAdvance<int>();
            this.Count3 = data.ReadAndAdvance<int>();

            this.Quad1 = data.ReadAndAdvance<Quaternion>(this.Count1);
            this.Quad2 = data.ReadAndAdvance<Quaternion>(this.Count2);
            this.Vec3 = data.ReadAndAdvance<Vector3>(this.Count3);

            this.MeshCount = data.ReadAndAdvance<int>();
            this.Meshes = new Mesh[this.MeshCount];
            for (var i = 0; i < this.MeshCount; i++)
                this.Meshes[i] = new(ref data);
        }

        public struct Mesh {
            public int VertexCount;
            public int IndexCount;
            public float[] Unknown;
            public string FileName;
            public VertexSet[] Vertices;
            public IndexSet[] Indices;

            public Mesh(ref ReadOnlySpan<byte> data) {
                this.VertexCount = data.ReadAndAdvance<int>();
                this.IndexCount = data.ReadAndAdvance<int>();
                this.Unknown = data.ReadAndAdvance<float>(0x11);
                this.FileName = data.ReadAndAdvanceString();
                this.Vertices = data.ReadAndAdvance<VertexSet>(this.VertexCount);
                this.Indices = data.ReadAndAdvance<IndexSet>(this.IndexCount);
            }

            public struct VertexSet {
                public int Value1;
                public int Value2;
                public int Value3;
                public int Value4;
                public int Value5;
            }

            public struct IndexSet {
                public short Value1;
                public short Value2;
                public short Value3;
            }
        }
    }

    public struct AnimationKmpChunk {
        public const uint ChunkMagic = 0x6B6D0000;

        public int Count1;

        public AnimationKmpChunk(ref ReadOnlySpan<byte> data) {
            if (data.ReadAndAdvance<uint>() != (ChunkMagic | 8))
                throw new InvalidDataException();

            this.Count1 = data.ReadAndAdvance<int>();
            for (var i = 0; i < this.Count1; i++)
                _ = new Inner(ref data);
        }

        public struct Inner {
            public int AnimationCount;
            public float Value1;
            public float Value2;
            public byte Byte1;
            public float Value3;
            public float Value4;
            public float Value5;

            public int SmthCount2;
            public Inner2[] Smth2;
            
            public int SmthCount3;
            public Inner2[] Smth3;
            
            public BoneAnimation[] Animations;
            
            public Inner(ref ReadOnlySpan<byte> data) {
                this.AnimationCount = data.ReadAndAdvance<int>();
                this.Value1 = data.ReadAndAdvance<float>();
                this.Value2 = data.ReadAndAdvance<float>();
                this.Byte1 = data.ReadAndAdvance<byte>();
                this.Value3 = data.ReadAndAdvance<float>();
                this.Value4 = data.ReadAndAdvance<float>();
                this.Value5 = data.ReadAndAdvance<float>();

                this.SmthCount2 = data.ReadAndAdvance<int>();
                this.Smth2 = data.ReadAndAdvance<Inner2>(this.SmthCount2);

                this.SmthCount3 = data.ReadAndAdvance<int>();
                this.Smth3 = data.ReadAndAdvance<Inner2>(this.SmthCount3);

                this.Animations = new BoneAnimation[this.AnimationCount];
                for (var i = 0; i < this.AnimationCount; i++)
                    this.Animations[i] = new(ref data);
            }

            public struct Inner2 {
                public float Value1;
                public int Value2;
                public float Value3;
            }

            public struct BoneAnimation {
                public int BoneIndex; // probably
                public int Count;
                public float[] Times;
                public Vector3[] Scales;
                public Quaternion[] Rotations;
                public Vector3[] Translations;
                
                public BoneAnimation(ref ReadOnlySpan<byte> data) { 
                    this.BoneIndex = data.ReadAndAdvance<int>();
                    this.Count = data.ReadAndAdvance<int>();
                    this.Times = new float[this.Count];
                    this.Scales = new Vector3[this.Count];
                    this.Rotations = new Quaternion[this.Count];
                    this.Translations = new Vector3[this.Count];
                    for (var i = 0; i < this.Count; i++) {
                        this.Times[i] = data.ReadAndAdvance<float>();
                        this.Scales[i] = data.ReadAndAdvance<Vector3>();
                        this.Rotations[i] = data.ReadAndAdvance<Quaternion>();
                        this.Translations[i] = data.ReadAndAdvance<Vector3>();
                    }
                }
            }
        }
    }

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

        public struct Inner {
            public float Duration;
            public int Count;
            public Inner2[] Values;
            
            public Inner(ref ReadOnlySpan<byte> data) {
                this.Duration = data.ReadAndAdvance<float>();
                this.Count = data.ReadAndAdvance<int>();
                this.Values = data.ReadAndAdvance<Inner2>(this.Count);
            }

            public struct Inner2 {
                public int Value1;
                public float Value2;
                public float Value3;
                public float Value4;
                public float Value5;
                public float Value6;
                public float Value7;
                public float Value8;
                public float Value9;
                public float ValueA;
            }
        }
    }

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

            public struct Inner2 {
                public int Value1;
                public int Value2;
                public Vector3 Value3;
                public float Value4;
                public int Value5;
                public Vector3[] Value6;
                public Inner3[] Value7;
                
                public Inner2(ref ReadOnlySpan<byte> data) {
                    this.Value1 = data.ReadAndAdvance<int>();
                    this.Value2 = data.ReadAndAdvance<int>();
                    this.Value3 = data.ReadAndAdvance<Vector3>();
                    this.Value4 = data.ReadAndAdvance<float>();
                    this.Value5 = data.ReadAndAdvance<int>();
                    this.Value6 = data.ReadAndAdvance<Vector3>(this.Value1);
                    this.Value7 = data.ReadAndAdvance<Inner3>(this.Value2);
                }

                public struct Inner3 {
                    public int Value1;
                    public int Value2;
                    public int Value3;
                    public float Value4;
                }
            }
        }
    }

    public struct KmpFile {
        public SkeletonKmpChunk Skeleton;
        public ModelKmpChunk Model;
        public AnimationKmpChunk Animation;
        public ChainingMotionKmpChunk ChainingMotion;
        public CollisionMeshKmpChunk Data6B43;

        public KmpFile(ReadOnlySpan<byte> data) {
            while (!data.IsEmpty) {
                var chunkType = data.ReadAndAdvance<uint>();
                switch (chunkType) {
                    case 0x6B4D0008:
                        // begin
                        break;
                    case 0x2F6B4D00:
                        // end
                        break;
                    case SkeletonKmpChunk.ChunkMagic:
                        this.Skeleton = new(ref data);
                        break;
                    case ModelKmpChunk.ChunkMagic:
                        this.Model = new(ref data);
                        break;
                    case AnimationKmpChunk.ChunkMagic:
                        this.Animation = new(ref data);
                        break;
                    case ChainingMotionKmpChunk.ChunkMagic:
                        this.ChainingMotion = new(ref data);
                        break;
                    case CollisionMeshKmpChunk.ChunkMagic:
                        this.Data6B43 = new(ref data);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }

    public static void Main() {
        var src = new[] {
            new KmpFile(File.ReadAllBytes(@"Z:\data1\Program_Executable_Files\Web\Gd.kmp")),
            new KmpFile(File.ReadAllBytes(@"Z:\data1\Program_Executable_Files\Web\Ga.kmp")),
            new KmpFile(File.ReadAllBytes(@"Z:\data1\Program_Executable_Files\Web\Dr.kmp")),
            new KmpFile(File.ReadAllBytes(@"Z:\data1\Program_Executable_Files\Web\Wb.kmp")),
        };
    }

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
