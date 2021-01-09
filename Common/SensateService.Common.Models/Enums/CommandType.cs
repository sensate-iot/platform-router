/*
 * Authorization service commands.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateService.Common.Data.Enums
{
	public enum CommandType
	{
		FlushUser,
		FlushSensor,
		FlushKey,
		AddUser,
		AddSensor,
		AddKey,
		AddLiveDataSensor,
		RemoveLiveDataSensor,
		SyncLiveDataSensors
	}
}