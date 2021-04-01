using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BridgeMonitor
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class Boat
    {
        [JsonPropertyName("boat_name")]
        public string BoatName { get; set; }

        [JsonPropertyName("closing_type")]
        public string ClosingType { get; set; }

        [JsonPropertyName("closing_date")]
        public DateTime ClosingDate { get; set; }

        [JsonPropertyName("reopening_date")]
        public DateTime ReopeningDate { get; set; }
    }

}
