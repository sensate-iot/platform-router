/*
 * Abstract hash algorithm.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Text.RegularExpressions;

namespace SensateService.Crypto
{
	public interface IHashAlgorithm
	{
		Regex GetMatchRegex();
		Regex GetSearchRegex();
		byte[] ComputeHash(byte[] input);
	}
}