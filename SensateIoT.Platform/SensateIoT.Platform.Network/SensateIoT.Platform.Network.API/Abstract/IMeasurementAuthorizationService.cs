/*
 * Measurement authorization interface.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading.Tasks;
using SensateIoT.Platform.Network.API.DTO;

namespace SensateIoT.Platform.Network.API.Abstract
{
	public interface IMeasurementAuthorizationService
	{
		void AddMessage(JsonMeasurement data);
		Task<int> ProcessAsync();
	}
}