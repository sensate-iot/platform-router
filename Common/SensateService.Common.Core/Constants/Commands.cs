/*
 * Authorization service commands.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateService.Constants
{
	public class Commands
	{
		public const string FlushUser = "flush_user";
		public const string FlushSensor = "flush_sensor";
		public const string FlushKey = "flush_key";

		public const string AddUser = "add_user";
		public const string AddSensor = "add_sensor";
		public const string AddKey = "add_key";

		public const string CommandKey = "cmd";
		public const string ArgumentKey = "args";
	}
}
