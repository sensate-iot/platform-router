/*
 * API key DTO model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

namespace SensateIoT.Platform.Network.Data.DTO
{
	public class ApiKey
	{
		public Guid AccountID { get; set; }
		public bool IsRevoked { get; set; }
		public bool IsReadOnly { get; set; }
	}
}
