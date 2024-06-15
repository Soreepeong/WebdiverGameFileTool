using WebdiverGameFileTool.FileFormats;

namespace WebdiverGameFileTool;

public static class Program {
    public static void Main() {
        const string rootDir = @".\Data\Input\Web";
        const string outDir = @".\Data\Output";
        var src = new[] {
            new KmpFile(File.ReadAllBytes(Path.Join(rootDir, "Gd.kmp"))),
            // new KmpFile(File.ReadAllBytes(@"Z:\data1\Program_Executable_Files\Web\Ga.kmp")),
            // new KmpFile(File.ReadAllBytes(@"Z:\data1\Program_Executable_Files\Web\Dr.kmp")),
            // new KmpFile(File.ReadAllBytes(@"Z:\data1\Program_Executable_Files\Web\Wb.kmp")),
        };
        src[0].Export(
            rootDir,
            outDir,
            "gd");
    }
}
