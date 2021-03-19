/*
 * Statistics type definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.Platform.Network.Data.Enums
{
	public enum StatisticsType
	{
		HttpGet,
		HttpPost,
		Email,
		SMS,
		LiveData,
		MQTT,
		ControlMessage,
		MeasurementStorage,
		MessageStorage,
		MessageRouted
	}
}
