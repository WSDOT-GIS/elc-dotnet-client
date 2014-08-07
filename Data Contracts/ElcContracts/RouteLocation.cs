using System;
using System.Runtime.Serialization;
using Wsdot.Geometry.Contracts;

namespace Wsdot.Elc.Contracts
{
	/// <summary>
	/// Represents a location on a state route.
	/// </summary>
	[DataContract]
	public class RouteLocation
	{
		/// <summary>
		/// Since the Find Nearest Route Location method does not return records for locations where it could not find any routes within the search parameters,
		/// this ID parameter can be used to indicate which source location a route location corresponds to.
		/// </summary>
		[DataMember(EmitDefaultValue=false)]
		public int? Id { get; set; }

		/// <summary>An 3 to 11 digit state route identifier.</summary>
		[DataMember(EmitDefaultValue = false)]
		public string Route { get; set; }

		/// <summary>The starting measure value.  This is known as a measure or M value outside of WSDOT.</summary>
		[DataMember(EmitDefaultValue = false)]
		public float? Arm { get; set; }

		/// <summary>The SRMP for the start point of a route segment or the only point of a point.</summary>
		[DataMember(EmitDefaultValue = false)]
		public float? Srmp { get; set; }

		/// <summary>Indicates if the SRMP value is back mileage.</summary>
		[DataMember(EmitDefaultValue = false)]
		public bool? Back { get; set; }

		/// <summary>Indicates of this location is on the Decrease LRS.  This value will be ignored if route is a ramp.</summary>
		[DataMember(EmitDefaultValue = false)]
		public bool? Decrease { get; set; }

		/// <summary>The date that the data was collected.</summary>
		[IgnoreDataMember]
		public DateTime? ReferenceDate { get; set; }

		/// <summary>The ArmCalc output date.</summary>
		[IgnoreDataMember]
		public DateTime? ResponseDate { get; set; }

		/// <summary>The end measure value of a line segment.  This is known as a measure or M value outside of WSDOT.</summary>
		[DataMember(EmitDefaultValue = false)]
		public float? EndArm { get; set; }

		/// <summary>The end SRMP value of a line segment.</summary>
		[DataMember(EmitDefaultValue = false)]
		public float? EndSrmp { get; set; }

		/// <summary>Indicates if endsrmp represents back-mileage.</summary>
		[DataMember(EmitDefaultValue = false)]
		public bool? EndBack { get; set; }

		/// <summary>The date that endarm and/or endsrmp was collected.</summary>
		[IgnoreDataMember]
		public DateTime? EndReferenceDate { get; set; }

		/// <summary>The ArmCalc output date for the end point.</summary>
		[IgnoreDataMember]
		public DateTime? EndResponseDate { get; set; }

		/// <summary>This is for storing ArmCalc result data of the start point.</summary>
		[IgnoreDataMember]
		public DateTime? RealignmentDate { get; set; }

		/// <summary>This is for storing ArmCalc result data of the end point.</summary>
		[IgnoreDataMember]
		public DateTime? EndRealignDate { get; set; }


		/// <summary>Gets or sets <see cref="ReferenceDate"/> using a string.</summary>
		[DataMember(Name="ReferenceDate", EmitDefaultValue = false)]
		public string ReferenceDateAsString
		{
			get
			{
				return DateToString(ReferenceDate);
			}
			set
			{
				ReferenceDate = StringToDate(value);
			}
		}

		/// <summary>The ArmCalc output date.</summary>
		[DataMember(Name="ResponseDate", EmitDefaultValue = false)]
		public string ResponseDateAsString
		{
			get
			{
				return DateToString(ResponseDate);
			}
			set
			{
				ResponseDate = StringToDate(value);
			}
		}


		/// <summary>The date that endarm and/or endsrmp was collected.</summary>
		[DataMember(Name="EndReferenceDate", EmitDefaultValue = false)]
		public string EndReferenceDateAsString {
			get
			{
				return DateToString(EndReferenceDate);
			}
			set
			{
				EndReferenceDate = StringToDate(value);
			}
		}

		/// <summary>The ArmCalc output date for the end point.</summary>
		[DataMember(Name="EndResponseDate", EmitDefaultValue = false)]
		public string EndResponseDateAsString
		{
			get
			{
				return DateToString(EndResponseDate);
			}
			set
			{
				EndResponseDate = StringToDate(value);
			}
		}

		/// <summary>This is for storing ArmCalc result data of the start point.</summary>
		[DataMember(Name="RealignmentDate", EmitDefaultValue = false)]
		public string RealignmentDateAsString
		{
			get
			{
				return DateToString(RealignmentDate);
			}
			set
			{
				RealignmentDate = StringToDate(value);
			}
		}

		/// <summary>This is for storing ArmCalc result data of the end point.</summary>
		[DataMember(Name="EndRealignDate", EmitDefaultValue = false)]
		public string EndRealignDateAsString
		{
			get
			{
				return DateToString(EndRealignDate);
			}
			set
			{
				EndRealignDate = StringToDate(value);
			}
		}




		/// <summary>Return code from ArmCalc. <seealso href="http://wwwi.wsdot.wa.gov/gis/roadwaydata/training/roadwaydata/pdf/PC_ArmCalc_Manual_3-19-2009.pdf">Appendix A of the PC ArmCalc Training Manual</seealso></summary>
		[DataMember(EmitDefaultValue = false)]
		public int? ArmCalcReturnCode { get; set; }

		/// <summary>Return code from ArmCalc. <seealso href="http://wwwi.wsdot.wa.gov/gis/roadwaydata/training/roadwaydata/pdf/PC_ArmCalc_Manual_3-19-2009.pdf">Appendix A of the PC ArmCalc Training Manual</seealso></summary>
		[DataMember(EmitDefaultValue = false)]
		public int? ArmCalcEndReturnCode { get; set; }

		/// <summary>
		/// The error message (if any) returned by ArmCalc when converting the begin point.
		/// </summary>
		[DataMember(EmitDefaultValue = false)]
		public string ArmCalcReturnMessage { get; set; }

		/// <summary>
		/// The error message (if any) returned by ArmCalc when converting the end point.
		/// </summary>
		[DataMember(EmitDefaultValue = false)]
		public string ArmCalcEndReturnMessage { get; set; }

		/// <summary>
		/// If a location cannot be found on the LRS, this value will contain a message.
		/// </summary>
		[DataMember(EmitDefaultValue = false)]
		public string LocatingError { get; set; }

		/// <summary>
		/// A <see cref="PointContract">point</see> or <see cref="PolylineContract">line</see> on a route.
		/// </summary>
		[DataMember(EmitDefaultValue = false)]
		public GeometryContract RouteGeometry { get; set; }

		/// <summary>
		/// When locating the nearest point along a route, this value will be set to the input point.
		/// </summary>
		[DataMember(EmitDefaultValue = false)]
		public PointContract EventPoint { get; set; }

		/// <summary>
		/// The offset distance from the <see cref="RouteLocation.EventPoint"/> to the <see cref="RouteLocation.RouteGeometry"/> point.
		/// </summary>
		[DataMember(EmitDefaultValue = false)]
		public double? Distance { get; set; }

		/// <summary>
		/// The offset angle from the <see cref="RouteLocation.EventPoint"/> to the <see cref="RouteLocation.RouteGeometry"/> point.
		/// </summary>
		[DataMember(EmitDefaultValue = false)]
		public double? Angle { get; set; }

		/// <summary>
		/// Returns <see langword="true"/> if this location is a line, <see langword="false"/> if it is a point.
		/// </summary>
		[IgnoreDataMember]
		public bool IsLine
		{
			get
			{
				return (EndArm.HasValue || EndSrmp.HasValue);
			}
		}

		private static DateTime? StringToDate(string dateStr)
		{
			if (string.IsNullOrEmpty(dateStr))
			{
				return null;
			}
			DateTime date;
			if (DateTime.TryParse(dateStr, out date))
			{
				return date;
			}
			else
			{
				return null;
			}
		}

		private static string DateToString(DateTime? date)
		{
			return date.HasValue && date.Value > DateTime.MinValue ? date.Value.ToShortDateString() : null;
		}
	}
}
