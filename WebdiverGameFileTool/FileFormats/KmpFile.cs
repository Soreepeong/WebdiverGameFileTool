using System.Numerics;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using WebdiverGameFileTool.FileFormats.GltfInterop;
using WebdiverGameFileTool.FileFormats.GltfInterop.Models;
using WebdiverGameFileTool.FileFormats.KmpChunks;
using WebdiverGameFileTool.Util;
using WebdiverGameFileTool.Util.MathExtras;

namespace WebdiverGameFileTool.FileFormats;

public class KmpFile {
    public readonly SkeletonKmpChunk Skeleton;
    public readonly ModelKmpChunk Model;
    public readonly AnimationKmpChunk Animation;
    public readonly ChainingMotionKmpChunk ChainingMotion;
    public readonly CollisionMeshKmpChunk CollisionMesh;

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
                    this.CollisionMesh = new(ref data);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public void Export(string rootDir, string outDir, string outName) {
        var gltfRoot = new GltfTuple();
        gltfRoot = new();
        gltfRoot.Root.ExtensionsUsed.Add("KHR_materials_specular");
        gltfRoot.Root.ExtensionsUsed.Add("KHR_materials_pbrSpecularGlossiness");
        gltfRoot.Root.ExtensionsUsed.Add("KHR_materials_emissive_strength");

        var rootNode = new GltfNode();
        gltfRoot.Root.Scenes[gltfRoot.Root.Scene].Nodes.Add(gltfRoot.Root.Nodes.AddAndGetIndex(rootNode));

        var skin = new GltfSkin {Joints = []};
        rootNode.Skin = gltfRoot.Root.Skins.AddAndGetIndex(skin);

        Span<int> boneToNode = stackalloc int[this.Skeleton.BoneCount];
        var trsabs = new Matrix4x4[this.Skeleton.BoneCount];
        for (var i = 0; i < this.Skeleton.BoneCount; i++) {
            if (!Matrix4x4.Invert(this.Skeleton.TrsInverseRelative.GetComposed(i), out var rel)
                || !Matrix4x4.Decompose(rel, out var s, out var q, out var t))
                throw new InvalidOperationException();

            trsabs[i] = Matrix4x4.Invert(
                Matrix4x4.CreateScale(s) *
                Matrix4x4.CreateFromQuaternion(q) *
                Matrix4x4.CreateTranslation(t)
                , out var tt)
                ? tt
                : throw new InvalidOperationException();
            if (this.Skeleton.BoneParents[i] != -1)
                trsabs[i] = trsabs[this.Skeleton.BoneParents[i]] * trsabs[i];

            var nodeIndex = gltfRoot.Root.Nodes.AddAndGetIndex(new() {
                Name = $"Bone{i}",
                Children = [],
                Translation = t.ToFloatList(Vector3.Zero),
                Rotation = q.ToFloatList(Quaternion.Identity),
                Scale = s.ToFloatList(Vector3.One),
            });
            skin.Joints.Add(nodeIndex);
            boneToNode[i] = nodeIndex;

            if (this.Skeleton.BoneParents[i] == -1)
                rootNode.Children.Add(nodeIndex);
            else
                gltfRoot.Root.Nodes[boneToNode[this.Skeleton.BoneParents[i]]].Children.Add(nodeIndex);
        }

        skin.InverseBindMatrices = gltfRoot.AddAccessor(
            null,
            Enumerable.Range(0, this.Skeleton.BoneCount)
                .Select(boneIndex => trsabs[boneIndex].Normalize()).ToArray()
                .AsSpan());

        var mesh = new GltfMesh();
        rootNode.Mesh = gltfRoot.Root.Meshes.AddAndGetIndex(mesh);

        foreach (var materialAndMesh in this.Model.Meshes) {
            mesh.Primitives.Add(new() {
                Attributes = new() {
                    Position = gltfRoot.AddAccessor(
                        null,
                        materialAndMesh.Vertices.Select(x => this.Model.Vertices[x.VertexIndex].Value1).ToArray()
                            .AsSpan(),
                        target: GltfBufferViewTarget.ArrayBuffer),
                    Normal = gltfRoot.AddAccessor(
                        null,
                        materialAndMesh.Vertices
                            .Select(x => Vector3.Normalize(this.Model.Normals[x.NormalIndex].Value1)).ToArray()
                            .AsSpan(),
                        target: GltfBufferViewTarget.ArrayBuffer),
                    TexCoord0 = gltfRoot.AddAccessor(
                        null,
                        materialAndMesh.Vertices.Select(x => x.Uv).ToArray().AsSpan(),
                        target: GltfBufferViewTarget.ArrayBuffer),
                    Weights0 = gltfRoot.AddAccessor(
                        null,
                        materialAndMesh.Vertices.Select(_ => new Vector4(1, 0, 0, 0)).ToArray().AsSpan(),
                        target: GltfBufferViewTarget.ArrayBuffer),
                    Joints0 = gltfRoot.AddAccessor(
                        null,
                        materialAndMesh.Vertices.Select(
                                x => new Vector4<ushort>((ushort) this.Model.Data3[x.Data3Index].Value1, 0, 0, 0))
                            .ToArray()
                            .AsSpan(),
                        target: GltfBufferViewTarget.ArrayBuffer),
                },
                Indices = gltfRoot.AddAccessor(
                    null,
                    MemoryMarshal.Cast<ModelKmpChunk.Mesh.IndexSet, ushort>(materialAndMesh.Indices),
                    target: GltfBufferViewTarget.ElementArrayBuffer),
                Material = gltfRoot.Root.Materials.AddAndGetIndex(new() {
                    PbrMetallicRoughness = new() {
                        BaseColorTexture = string.IsNullOrWhiteSpace(materialAndMesh.FileName)
                            ? null
                            : new GltfTextureInfo {
                                Index = gltfRoot.AddTexture(
                                    materialAndMesh.FileName,
                                    Image.Load<Bgra32>(Path.Join(rootDir, materialAndMesh.FileName)),
                                    PngColorType.Rgb)
                            },
                    }
                }),
            });
        }

        foreach (var animation in this.Animation.Animations) {
            var target = new GltfAnimation();
            foreach (var track in animation.Tracks) {
                var times = gltfRoot.AddAccessor(null, track.Times.AsSpan());
                target.Channels.Add(new() {
                    Sampler = target.Samplers.AddAndGetIndex(
                        new() {
                            Input = times,
                            Output = gltfRoot.AddAccessor(null, track.Trs.Translation.AsSpan()),
                            Interpolation = GltfAnimationSamplerInterpolation.Linear,
                        }),
                    Target = new() {
                        Node = boneToNode[track.BoneIndex],
                        Path = GltfAnimationChannelTargetPath.Translation,
                    },
                });
                target.Channels.Add(new() {
                    Sampler = target.Samplers.AddAndGetIndex(
                        new() {
                            Input = times,
                            Output = gltfRoot.AddAccessor(null, track.Trs.Rotation.AsSpan()),
                            Interpolation = GltfAnimationSamplerInterpolation.Linear,
                        }),
                    Target = new() {
                        Node = boneToNode[track.BoneIndex],
                        Path = GltfAnimationChannelTargetPath.Rotation,
                    },
                });
                target.Channels.Add(new() {
                    Sampler = target.Samplers.AddAndGetIndex(
                        new() {
                            Input = times,
                            Output = gltfRoot.AddAccessor(null, track.Trs.Scale.AsSpan()),
                            Interpolation = GltfAnimationSamplerInterpolation.Linear,
                        }),
                    Target = new() {
                        Node = boneToNode[track.BoneIndex],
                        Path = GltfAnimationChannelTargetPath.Scale,
                    },
                });
            }

            gltfRoot.Root.Animations.Add(target);
        }
        
        gltfRoot.CompileSingleBufferToFiles(outDir, outName, default).Wait();
    }
}
