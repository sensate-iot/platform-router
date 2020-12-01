/*
 * Authorization service implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Security.Cryptography;
using System.Text;

using SensateIoT.Platform.Network.Data.DTO;

namespace SensateIoT.Platform.Network.Common.Services.Processing
{
	public class AuthorizationService : IAuthorizationService
	{
		public void SignControlMessage(ControlMessage message, string json)
		{
			using var sha = SHA256.Create();
			var binary = Encoding.ASCII.GetBytes(json);
			var hash = sha.ComputeHash(binary);

			message.Secret = BytesToHex(hash, false);
		}

		private static string BytesToHex(byte[] bytes, bool uppercase)
		{
			var builder = new StringBuilder(bytes.Length * 2);
			var format = uppercase ? "X2" : "x2";

			builder.Append('$');

			foreach(var b in bytes) {
				builder.Append(b.ToString(format));
			}

			builder.Append("==");

			return builder.ToString();
		}
	}
}
