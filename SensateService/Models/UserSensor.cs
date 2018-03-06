/*
 * Sensate user model.
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.IdentityModel;
using Microsoft.AspNetCore.Identity;

namespace SensateService.Models
{
	public class UserSensor
	{
		public string SensorId { get; set; }
		[ForeignKey("SensateUser")]
		public string UserId { get; set; }
		public bool Owner { get; set; }
	}
}