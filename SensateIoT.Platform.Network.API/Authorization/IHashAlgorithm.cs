using System.Text.RegularExpressions;

namespace SensateIoT.Platform.Network.API.Authorization
{
	public interface IHashAlgorithm
	{
		Regex GetMatchRegex();
		Regex GetSearchRegex();
		byte[] ComputeHash(byte[] input);
	}
}