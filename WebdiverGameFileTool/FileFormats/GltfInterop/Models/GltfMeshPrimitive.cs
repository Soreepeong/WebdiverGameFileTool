using Newtonsoft.Json;

namespace WebdiverGameFileTool.FileFormats.GltfInterop.Models;

public class GltfMeshPrimitive : BaseGltfObject {
    [JsonProperty("attributes")]
    public GltfMeshPrimitiveAttributes Attributes = new();

    [JsonProperty("indices", NullValueHandling = NullValueHandling.Ignore)]
    public int? Indices;

    [JsonProperty("material", NullValueHandling = NullValueHandling.Ignore)]
    public int? Material;
}
