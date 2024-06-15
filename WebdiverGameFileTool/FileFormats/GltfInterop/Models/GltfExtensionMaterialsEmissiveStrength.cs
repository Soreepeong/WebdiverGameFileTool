using Newtonsoft.Json;

namespace WebdiverGameFileTool.FileFormats.GltfInterop.Models;

public class GltfExtensionMaterialsEmissiveStrength : BaseGltfObject {
    [JsonProperty(
        "emissiveStrength",
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore)]
    public float? EmissiveStrength = 1f;
}