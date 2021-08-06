/*
 * Command enum.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.Platform.Router.Data.Enums
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
		SyncLiveDataSensors,
		DeleteUser
	}
}
