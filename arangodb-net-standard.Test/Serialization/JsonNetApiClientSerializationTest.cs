using ArangoDBNetStandard.Serialization;
using ArangoDBNetStandardTest.Serialization.Models;
using System.IO;
using System.Text;
using Xunit;

namespace ArangoDBNetStandardTest.Serialization
{
    public class JsonNetApiClientSerializationTest
    {
        [Fact]
        public void Serialize_ShouldSucceed()
        {
            var model = new TestModel()
            {
                NullPropertyToIgnore = null,
                PropertyToCamelCase = "myvalue"
            };

            var serialization = new JsonNetApiClientSerialization();

            byte[] jsonBytes = serialization.Serialize(model, true, true);

            string jsonString = Encoding.UTF8.GetString(jsonBytes);

            Assert.Contains("propertyToCamelCase", jsonString);
            Assert.DoesNotContain("nullPropertyToIgnore", jsonString);
        }

        [Fact]
        public void DeserializeFromStream_ShouldSucceed()
        {
            // Deserializing should match both "camelCase" and "CamelCase"

            byte[] jsonBytes = Encoding.UTF8.GetBytes(
                "{\"propertyToCamelCase\":\"myvalue\",\"NullPropertyToIgnore\":\"something\"}");

            var stream = new MemoryStream(jsonBytes);

            var serialization = new JsonNetApiClientSerialization();

            TestModel model = serialization.DeserializeFromStream<TestModel>(stream);

            Assert.Equal("myvalue", model.PropertyToCamelCase);
            Assert.Equal("something", model.NullPropertyToIgnore);
        }

        [Fact]
        public void Serialize_ShouldKeepUserModelsIntact_WhenTypeIsCursorOrTransactionBody()
        {
            var model = new TestModelToKeepWithDefault()
            {
                PropertyNameToKeepIntact = "something",
                NullPropertyToKeep = null,
                NullPropertyToIgnore = null
            };

            var body = new ArangoDBNetStandard.CursorApi.Models.PostCursorBody()
            {
                BindVars = new System.Collections.Generic.Dictionary<string, object>()
                {
                    { "MyModel", model },
                    { "MyBindingParam", "aParamValue" }
                }
            };

            var serialization = new JsonNetApiClientSerialization();

            byte[] jsonBytes = serialization.Serialize(body, true, true);

            string jsonString = Encoding.UTF8.GetString(jsonBytes);

            Assert.DoesNotContain("myModel", jsonString);
            Assert.DoesNotContain("myBindingParam", jsonString);
            Assert.DoesNotContain(nameof(TestModelToKeepWithDefault.NullPropertyToIgnore), jsonString);

            Assert.Contains("MyModel", jsonString);
            Assert.Contains("MyBindingParam", jsonString);
            Assert.Contains(nameof(TestModelToKeepWithDefault.PropertyNameToKeepIntact), jsonString);
            Assert.Contains(nameof(TestModelToKeepWithDefault.NullPropertyToKeep), jsonString);
        }
    }
}
