/*
 * Trigger handling service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SensateService.Common.Data.Models;

namespace SensateService.TriggerHandler.Services
{
	public interface ITriggerTextMatchingService
	{
		Task HandleTriggerAsync(IList<Tuple<Trigger, TriggerInvocation>> invocations);
	}
}