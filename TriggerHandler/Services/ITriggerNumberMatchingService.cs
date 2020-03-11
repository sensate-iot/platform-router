/*
 * Trigger handling service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SensateService.Models;

namespace SensateService.TriggerHandler.Services
{
	public interface ITriggerNumberMatchingService
	{
		Task HandleTriggerAsync(IList<Tuple<Trigger, TriggerInvocation, DataPoint>> invocations);
	}
}
