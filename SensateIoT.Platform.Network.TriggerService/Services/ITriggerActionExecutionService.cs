/*
 * Execute trigger actions.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;

using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.TriggerService.Services
{
	public interface ITriggerActionExecutionService
	{
		Task ExecuteAsync(TriggerAction action, string body);
	}
}
