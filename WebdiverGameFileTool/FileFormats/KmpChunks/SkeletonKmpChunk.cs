using WebdiverGameFileTool.Util;

namespace WebdiverGameFileTool.FileFormats.KmpChunks;

public readonly struct SkeletonKmpChunk {
    public const uint ChunkMagic = 0x6B530000;

    public readonly int BoneCount;
    public readonly int Unknown1;
    public readonly int[] BoneParents;
    public readonly TrsArray TrsInverseRelative;
    public readonly TrsArray TrsAbsolute;

    public SkeletonKmpChunk(ref ReadOnlySpan<byte> data) {
        if (data.ReadAndAdvance<uint>() != (ChunkMagic | 8))
            throw new InvalidDataException();
        this.BoneCount = data.ReadAndAdvance<int>();
        this.Unknown1 = data.ReadAndAdvance<int>();
        this.BoneParents = data.ReadAndAdvance<int>(this.BoneCount);
        this.TrsInverseRelative = new(ref data, this.BoneCount);
        this.TrsAbsolute = new(ref data, this.BoneCount);
    }
}
