/*
 * Authorization service interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using SensateIoT.Platform.Router.Data.DTO;

namespace SensateIoT.Platform.Router.Common.Services.Abstract
{
	public interface IAuthorizationService
	{
		void SignControlMessage(ControlMessage message, string json);
	}
}
