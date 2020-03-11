﻿/*
 * Internal measurement model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using SensateService.Models;

namespace SensateService.TriggerHandler.Models
{
	public class InternalBulkMeasurements
	{
		public IList<Measurement> Measurements { get; set; }
		public string CreatedBy { get; set; }
	}

	public class InternalMeasurement
	{
		public Measurement Measurement { get; set; }
		public string CreatedBy { get; set; }
	}
}

