/*
 * Abstract hash algorithm.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Text.RegularExpressions;

namespace SensateIoT.API.Common.Core.Crypto
{
	public interface IHashAlgorithm
	{
		Regex GetMatchRegex();
		Regex GetSearchRegex();
		byte[] ComputeHash(byte[] input);
	}
}