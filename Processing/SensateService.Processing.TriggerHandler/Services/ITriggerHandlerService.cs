/*
 * Trigger handling service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;
using SensateService.Common.Data.Models;
using SensateService.Common.IdentityData.Models;

namespace SensateService.Processing.TriggerHandler.Services
{
	public interface ITriggerHandlerService
	{
		Task HandleTriggerAction(SensateUser user, Trigger trigger, TriggerAction action, TriggerInvocation last, string body);
	}
}