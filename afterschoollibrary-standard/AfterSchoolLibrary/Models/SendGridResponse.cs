using Newtonsoft.Json;
using System.Collections.Generic;

namespace AfterSchoolLibrary.Models
{
    public class SendGridResponse
    {
        [JsonProperty("new_count")]
        public int NewCount { get; set; }

        [JsonProperty("updated_count")]
        public int UpdatedCount { get; set; }

        [JsonProperty("error_count")]
        public int ErrorCount { get; set; }

        [JsonProperty("error_indices")]
        public List<object> ErrorIndices { get; set; }

        [JsonProperty("unmodified_indices")]
        public List<object> UnmodifiedIndices { get; set; }

        [JsonProperty("persisted_recipients")]
        public List<string> PersistedRecipients { get; set; }

        [JsonProperty("errors")]
        public List<object> Errors { get; set; }
    }
}
