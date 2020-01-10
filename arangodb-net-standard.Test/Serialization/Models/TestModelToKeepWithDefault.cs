namespace ArangoDBNetStandardTest.Serialization.Models
{
    /// <summary>
    /// A test class to ensure <see cref="ArangoDBNetStandard.Serialization.JsonNetApiClientSerialization"/>
    /// uses default serialization for properties containing user models like 
    /// <see cref="ArangoDBNetStandard.CursorApi.Models.PostCursorBody"/>.
    /// </summary>
    public class TestModelToKeepWithDefault
    {
        public string PropertyNameToKeepIntact { get; set; }

        public string NullPropertyToKeep { get; set; }

        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string NullPropertyToIgnore { get; set; }
    }
}
