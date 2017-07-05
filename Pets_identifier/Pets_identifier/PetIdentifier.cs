using Newtonsoft.Json;

namespace Pets_identifier
{
	public class PetIdentifier
	{
		[JsonProperty(PropertyName = "id")]
		public string ID { get; set; }

		[JsonProperty(PropertyName = "pet")]
		public string Pet { get; set; }

		[JsonProperty(PropertyName = "itemCategory")]
		public string Category { get; set; }

		[JsonProperty(PropertyName = "itemLink")]
		public string Link { get; set; }
	}
}
