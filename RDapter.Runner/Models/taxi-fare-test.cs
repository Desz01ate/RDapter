// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a ModelGenerator.
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
using System;
namespace RDapter.Runner.Models
{
	//You can get Utilities package via nuget : Install-Package Deszolate.Utilities.Lite
	//[Utilities.Attributes.SQL.Table("taxi-fare-test")]
	public partial class taxifaretest
	{
		//[Utilities.Attributes.SQL.PrimaryKey]
		public string vendor_id { get; set; }
		public float? rate_code { get; set; }
		public float? passenger_count { get; set; }
		public float? trip_time_in_secs { get; set; }
		public float? trip_distance { get; set; }
		public string payment_type { get; set; }
		public float? fare_amount { get; set; }
	}
}

