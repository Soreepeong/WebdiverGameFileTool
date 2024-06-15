using Newtonsoft.Json;

namespace WebdiverGameFileTool.FileFormats.GltfInterop.Models;

public class GltfTextureInfo : BaseGltfObject, ICloneable {
    [JsonProperty("index", NullValueHandling = NullValueHandling.Ignore)]
    public int? Index;

    [JsonProperty("texCoord", NullValueHandling = NullValueHandling.Ignore)]
    public int? TexCoord;

    public object Clone() => MemberwiseClone();
}
