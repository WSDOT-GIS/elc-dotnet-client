using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Wsdot.Elc.Contracts
{
	[DataContract]
	public class MapServerInfo
	{
		[DataMember(Name="layers")]
		public LayerInfo[] Layers { get; set; }
	}
}
