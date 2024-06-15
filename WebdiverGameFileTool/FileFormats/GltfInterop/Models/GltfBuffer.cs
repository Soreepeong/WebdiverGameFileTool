using Newtonsoft.Json;

namespace WebdiverGameFileTool.FileFormats.GltfInterop.Models;

public class GltfBuffer : BaseGltfObject {
    [JsonProperty("byteLength")]
    public long ByteLength;

    [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
    public string? Uri;
}
