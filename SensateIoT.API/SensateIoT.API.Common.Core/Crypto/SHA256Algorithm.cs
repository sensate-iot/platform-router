/*
 * SHA256 hash algorithm.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SensateIoT.API.Common.Core.Crypto
{
	public class SHA256Algorithm : IHashAlgorithm
	{
		private static readonly Regex ShaRegex =
			new Regex(@"^(\$[0-f0-9]{64}==)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex SecretRegex =
			new Regex(@"\$[a-f0-9]{64}==", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		public Regex GetMatchRegex()
		{
			return ShaRegex;
		}

		public Regex GetSearchRegex()
		{
			return SecretRegex;
		}

		public byte[] ComputeHash(byte[] input)
		{
			using var sha = SHA256.Create();
			return sha.ComputeHash(input);
		}
	}
}
