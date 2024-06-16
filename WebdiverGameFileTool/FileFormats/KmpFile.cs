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

    public GltfTuple Export(string rootDir) {
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
        for (var i = 0; i < this.Skeleton.BoneCount; i++) {
            if (!Matrix4x4.Decompose(
                    this.Skeleton.TrsInverseRelative.GetComposed(i).InvertOrThrow(),
                    out var s,
                    out var r,
                    out var t)) {
                throw new InvalidOperationException();
            }

            var nodeIndex = gltfRoot.Root.Nodes.AddAndGetIndex(new() {
                Name = $"Bone{i}",
                Children = [],
                Translation = t.ToFloatList(Vector3.Zero),
                Rotation = r.ToFloatList(Quaternion.Identity),
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
                .Select(boneIndex => this.Skeleton.TrsAbsolute.GetComposed(boneIndex).InvertOrThrow().Normalize())
                .ToArray()
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

        var timeBuf = new List<float>();
        var translationBuf = new List<Vector3>();
        var rotationBuf = new List<Quaternion>();
        var scaleBuf = new List<Vector3>();

        for (var i = 0; i < this.ChainingMotion.Count; i++) {
            var chainingMotion = this.ChainingMotion.Values[i];
            var target = new GltfAnimation {
                Name = $"chain{i}",
            };
            for (var boneIndex = 0; boneIndex < this.Skeleton.BoneCount; boneIndex++) {
                timeBuf.Clear();
                translationBuf.Clear();
                rotationBuf.Clear();
                scaleBuf.Clear();
                foreach (var cm in chainingMotion.Values) {
                    var animation = this.Animation.Animations[cm.AnimationIndex];
                    if (!animation.TryFindTrack(boneIndex, out var track))
                        continue;

                    var start = timeBuf.Count;
                    CollectionsMarshal.SetCount(timeBuf, start + track.FrameCount);
                    CollectionsMarshal.SetCount(translationBuf, start + track.FrameCount);
                    CollectionsMarshal.SetCount(rotationBuf, start + track.FrameCount);
                    CollectionsMarshal.SetCount(scaleBuf, start + track.FrameCount);
                    
                    var times = CollectionsMarshal.AsSpan(timeBuf)[start..];
                    var translations = CollectionsMarshal.AsSpan(translationBuf)[start..];
                    var rotations = CollectionsMarshal.AsSpan(rotationBuf)[start..];
                    var scales = CollectionsMarshal.AsSpan(scaleBuf)[start..];
                    for (var j = 0; j < track.FrameCount; j++) {
                        times[j] = cm.BeginTime + track.FrameTimes[j] / cm.PlaybackSpeed;
                        if (!Matrix4x4.Decompose(
                                track.FrameTrs.GetComposed(j).InvertOrThrow(),
                                out scales[j],
                                out rotations[j],
                                out translations[j]))
                            throw new InvalidOperationException();
                    }
                }

                var inputAccessor = gltfRoot.AddAccessor(null, CollectionsMarshal.AsSpan(timeBuf));
                target.Channels.Add(new() {
                    Sampler = target.Samplers.AddAndGetIndex(
                        new() {
                            Input = inputAccessor,
                            Output = gltfRoot.AddAccessor(null, CollectionsMarshal.AsSpan(translationBuf)),
                            Interpolation = GltfAnimationSamplerInterpolation.Linear,
                        }),
                    Target = new() {
                        Node = boneToNode[boneIndex],
                        Path = GltfAnimationChannelTargetPath.Translation,
                    },
                });
                target.Channels.Add(new() {
                    Sampler = target.Samplers.AddAndGetIndex(
                        new() {
                            Input = inputAccessor,
                            Output = gltfRoot.AddAccessor(null, CollectionsMarshal.AsSpan(rotationBuf)),
                            Interpolation = GltfAnimationSamplerInterpolation.Linear,
                        }),
                    Target = new() {
                        Node = boneToNode[boneIndex],
                        Path = GltfAnimationChannelTargetPath.Rotation,
                    },
                });
                target.Channels.Add(new() {
                    Sampler = target.Samplers.AddAndGetIndex(
                        new() {
                            Input = inputAccessor,
                            Output = gltfRoot.AddAccessor(null, CollectionsMarshal.AsSpan(scaleBuf)),
                            Interpolation = GltfAnimationSamplerInterpolation.Linear,
                        }),
                    Target = new() {
                        Node = boneToNode[boneIndex],
                        Path = GltfAnimationChannelTargetPath.Scale,
                    },
                });
            }

            gltfRoot.Root.Animations.Add(target);
        }
        
        for (var i = 0; i < this.Animation.Count; i++) {
            var animation = this.Animation.Animations[i];
            var target = new GltfAnimation {
                Name = $"single{i}",
            };
            foreach (var track in animation.Tracks) {
                CollectionsMarshal.SetCount(translationBuf, track.FrameCount);
                CollectionsMarshal.SetCount(rotationBuf, track.FrameCount);
                CollectionsMarshal.SetCount(scaleBuf, track.FrameCount);

                var translations = CollectionsMarshal.AsSpan(translationBuf);
                var rotations = CollectionsMarshal.AsSpan(rotationBuf);
                var scales = CollectionsMarshal.AsSpan(scaleBuf);
                for (var j = 0; j < track.FrameCount; j++) {
                    if (!Matrix4x4.Decompose(
                            track.FrameTrs.GetComposed(j).InvertOrThrow(),
                            out scales[j],
                            out rotations[j],
                            out translations[j]))
                        throw new InvalidOperationException();
                }

                var inputAccessor = gltfRoot.AddAccessor(null, track.FrameTimes.AsSpan());

                target.Channels.Add(new() {
                    Sampler = target.Samplers.AddAndGetIndex(
                        new() {
                            Input = inputAccessor,
                            Output = gltfRoot.AddAccessor(null, translations),
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
                            Input = inputAccessor,
                            Output = gltfRoot.AddAccessor(null, rotations),
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
                            Input = inputAccessor,
                            Output = gltfRoot.AddAccessor(null, scales),
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

        return gltfRoot;
    }
}
