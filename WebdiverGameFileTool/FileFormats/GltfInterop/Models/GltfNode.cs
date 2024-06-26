﻿using JetBrains.Annotations;
using Newtonsoft.Json;

namespace WebdiverGameFileTool.FileFormats.GltfInterop.Models;

public class GltfNode : BaseGltfObject {
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name;

    [JsonProperty("mesh", NullValueHandling = NullValueHandling.Ignore)]
    public int? Mesh;

    [JsonProperty("skin", NullValueHandling = NullValueHandling.Ignore)]
    public int? Skin;

    [JsonProperty("children", NullValueHandling = NullValueHandling.Ignore)]
    public List<int> Children = new();

    [JsonProperty("rotation", NullValueHandling = NullValueHandling.Ignore)]
    public List<float>? Rotation;

    [JsonProperty("scale", NullValueHandling = NullValueHandling.Ignore)]
    public List<float>? Scale;

    [JsonProperty("translation", NullValueHandling = NullValueHandling.Ignore)]
    public List<float>? Translation;

    [JsonProperty("matrix", NullValueHandling = NullValueHandling.Ignore)]
    public List<float>? Matrix;

    [UsedImplicitly]
    public bool ShouldSerializeChildren() => Children.Any();
}
