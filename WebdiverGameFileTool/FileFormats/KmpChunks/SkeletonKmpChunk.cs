using System.Numerics;
using WebdiverGameFileTool.Util;

namespace WebdiverGameFileTool.FileFormats.KmpChunks;

public readonly struct SkeletonKmpChunk {
    public const uint ChunkMagic = 0x6B530000;

    public readonly int BoneCount;
    public readonly int Unknown1;
    public readonly int[] BoneParents;
    public readonly TrsArray Trs1;
    public readonly TrsArray Trs2;

    public SkeletonKmpChunk(ref ReadOnlySpan<byte> data) {
        if (data.ReadAndAdvance<uint>() != (ChunkMagic | 8))
            throw new InvalidDataException();
        this.BoneCount = data.ReadAndAdvance<int>();
        this.Unknown1 = data.ReadAndAdvance<int>();
        this.BoneParents = data.ReadAndAdvance<int>(this.BoneCount);
        this.Trs1 = new(ref data, this.BoneCount);
        this.Trs2 = new(ref data, this.BoneCount);
    }

    public Matrix4x4 GetRelativeBindPoseMatrix(in TrsArray trs, int boneIndex) =>
        Matrix4x4.CreateScale(trs.Scale[boneIndex]) *
        Matrix4x4.CreateFromQuaternion(trs.Rotation[boneIndex]) *
        Matrix4x4.CreateTranslation(trs.Translation[boneIndex]);

    public Matrix4x4 GetAbsoluteBindPoseMatrix(in TrsArray trs, int boneIndex) {
        if (this.BoneParents[boneIndex] == -1)
            return this.GetRelativeBindPoseMatrix(trs, boneIndex);
        return this.GetAbsoluteBindPoseMatrix(trs, this.BoneParents[boneIndex]) *
            this.GetRelativeBindPoseMatrix(trs, boneIndex);
    }

    public Matrix4x4 GetAbsoluteInverseBindPoseMatrix(in TrsArray trs, int boneIndex) =>
        Matrix4x4.Invert(this.GetAbsoluteBindPoseMatrix(trs, boneIndex), out var r) ? r : Matrix4x4.Identity;
}
