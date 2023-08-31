using Newtonsoft.Json;
using System.Collections.Generic;

namespace GaRyan2.SchedulesDirectAPI
{
    public class ProgramMetadata : BaseResponse
    {
        [JsonProperty("programID")]
        public string ProgramId { get; set; }

        [JsonProperty("data")]
        [JsonConverter(typeof(SingleOrListConverter<ProgramArtwork>))]
        public List<ProgramArtwork> Data { get; set; }
    }

    public class ProgramArtwork
    {
        private string _size;

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("size")]
        public string Size
        {
            get
            {
                if (!string.IsNullOrEmpty(_size)) return _size;
                switch (Width * Height)
                {
                    case 21600: // 2x3 (120 x 180)
                    case 24300: // 3x4 (135 x 180) and 4x3 (180 x 135)
                    case 32400: // 16x9 (480 x 270)
                        return "Sm";
                    case 86400: // 2x3 (240 x 360)
                    case 97200: // 3x4 (270 x 360) and 4x3 (360 x 270)
                    case 129600: // 16x9 (480 x 270)
                        return "Md";
                    case 345600: // 2x3 (480 x 720)
                    case 388800: // 3x4 (540 x 720) and 4x3 (720 x 540)
                    case 518400: // 16x9 (960 x 540)
                        return "Lg";
                }
                return _size;
            }
            set => _size = value;
        }

        [JsonProperty("aspect")]
        public string Aspect { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("primary")]
        public string Primary { get; set; }

        [JsonProperty("tier")]
        public string Tier { get; set; }
    }
}