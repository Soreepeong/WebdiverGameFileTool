using WebdiverGameFileTool.FileFormats;

namespace WebdiverGameFileTool;

public static class Program {
    public static void Main() {
        const string rootDir = @".\Data\Input\Web";
        const string outDir = @".\Data\Output";
        foreach (var k in new[] {"Gd", "Ga", "Dr", "Wb"})
            new KmpFile(File.ReadAllBytes(Path.Join(rootDir, $"{k}.kmp"))).Export(rootDir, outDir, k);
    }
}
