/*
 * Trigger handling service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;

using SensateService.Models;

namespace SensateService.TriggerHandler.Services
{
	public interface ITriggerHandlerService
	{
		Task HandleTriggerAction(SensateUser user, Trigger trigger, TriggerAction action, TriggerInvocation last, string body);
	}
}