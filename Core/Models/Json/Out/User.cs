/*
 * User viewmodel.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;

namespace SensateService.Models.Json.Out
{
	public class User
	{
		public string Id { get; set; }
		public string FirstName {get;set;}
		public string LastName {get;set;}
		public string Email {get;set;}
		public string PhoneNumber {get;set;}
		public DateTime RegisteredAt { get; set; }
	}
}
