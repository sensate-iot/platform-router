﻿/*
 * Authorization user model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateService.Common.Data.Dto.Authorization
{
	public class User
	{
		public string Id { get; set; }
		public bool BillingLockout { get; set; }
		public bool Banned { get; set; }
	}
}
