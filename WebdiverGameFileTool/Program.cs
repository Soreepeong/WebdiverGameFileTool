using WebdiverGameFileTool.FileFormats;

namespace WebdiverGameFileTool;

public static class Program {
    public static void Main() {
        const string rootDir = @".\Data\Input\Web";
        const string outDir = @".\Data\Output";

        foreach (var path in Directory.GetFiles(rootDir, "*.kmp")) {
            if (!path.EndsWith("\\ga.kmp", StringComparison.InvariantCultureIgnoreCase)) continue;
            new KmpFile(File.ReadAllBytes(path))
                .Export(rootDir)
                .CompileSingleBufferToFile(
                    Path.Join(outDir, Path.ChangeExtension(Path.GetFileNameWithoutExtension(path), ".glb")));
            // .CompileSingleBufferToFiles(outDir, Path.GetFileNameWithoutExtension(path), default).Wait();
        }
    }
}
