/*
 * API key authorization DTO model.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateService.Common.Data.Dto.Authorization
{
	public class ApiKey
	{
		public string Key { get; set; }
		public bool Revoked { get; set; }
		public bool ReadOnly { get; set; }
	}
}
