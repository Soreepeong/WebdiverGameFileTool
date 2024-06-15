using Newtonsoft.Json;

namespace WebdiverGameFileTool.FileFormats.GltfInterop.Models;

public class GltfExtensionMsftTextureDds : BaseGltfObject {
    [JsonProperty("source")]
    public int Source;
}
