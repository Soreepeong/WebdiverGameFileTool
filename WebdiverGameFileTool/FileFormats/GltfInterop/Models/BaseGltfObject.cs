using Newtonsoft.Json;

namespace WebdiverGameFileTool.FileFormats.GltfInterop.Models;

public class BaseGltfObject {
    [JsonProperty("extensions", NullValueHandling = NullValueHandling.Ignore)]
    public GltfExtensions? Extensions;
}
