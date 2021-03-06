﻿using Newtonsoft.Json;
using System;

namespace Reddit.Things
{
    [Serializable]
    public class WidgetImageProperties : BaseContainer
    {
        [JsonProperty("height")]
        public int Height;

        [JsonProperty("imageUrl")]
        public string ImageURL;

        [JsonProperty("width")]
        public int Width;
    }
}
