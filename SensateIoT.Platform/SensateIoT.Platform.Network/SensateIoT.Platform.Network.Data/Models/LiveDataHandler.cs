/*
 * Live data handler database model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.Platform.Network.Data.Models
{
	public class LiveDataHandler
	{
		public long ID { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }
	}
}
