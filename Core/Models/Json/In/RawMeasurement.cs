/*
 * Model representing minimal measurement information.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using Newtonsoft.Json.Linq;

namespace SensateService.Models.Json.In
{
	public class RawMeasurement
	{
		public JContainer Data {get;set;}
		public double Longitude {get;set;}
		public double Latitude {get;set;}
		public DateTime CreatedAt {get;set;}
		public string CreatedBySecret {get;set;}
		public string CreatedById { get; set; }

		public bool CreatedBy(Sensor sensor) => this.CreatedBySecret == sensor.Secret;
	}
}
