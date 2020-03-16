using System.Collections.Generic;
using System.Text.Json.Serialization;
using Sidekick.Business.Apis.Poe.Models;

namespace Sidekick.Business.Trades.Results
{
    public class ResultItem : Item
    {
        public bool Verified { get; set; }

        [JsonPropertyName("w")]
        public int Width { get; set; }

        [JsonPropertyName("h")]
        public int Height { get; set; }

        public string Icon { get; set; }

        public string League { get; set; }

        [JsonPropertyName("ilvl")]
        public int ItemLevel { get; set; }

        public string Note { get; set; }
        public List<LineContent> Properties { get; set; }
        public List<LineContent> Requirements { get; set; }
        public List<string> ImplicitMods { get; set; }
        public List<string> CraftedMods { get; set; }
        public List<string> ExplicitMods { get; set; }
        public List<string> UtilityMods { get; set; }

        [JsonPropertyName("frameType")]
        public Rarity Rarity { get; set; }

        public bool Corrupted { get; set; }
        public Extended Extended { get; set; }
    }
}