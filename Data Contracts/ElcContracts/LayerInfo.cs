using System.Runtime.Serialization;

namespace Wsdot.Elc.Contracts
{
	[DataContract(Name="layer")]
	public class LayerInfo
	{
		[DataMember(Name="id")]
		public int Id { get; set; }
		[DataMember(Name="name")]
		public string Name { get; set; }
		[DataMember(Name="parentLayerId")]
		public int ParentLayerId { get; set; }
		////[DataMember(Name="defaultVisibility")]
		////public bool defaultVisibility { get; set; }
		[DataMember(Name="subLayerIds")]
		public int[] SubLayerIds { get; set; }
		////[DataMember(Name="minScale")]
		////public int minScale { get; set; }
		////[DataMember(Name="maxScale")]
		////public int maxScale { get; set; }
	}
}
