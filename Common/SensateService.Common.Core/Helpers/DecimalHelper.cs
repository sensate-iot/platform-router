/*
 * System.Decimal extension methods.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using MongoDB.Bson;

namespace SensateService.Helpers
{
	public static class DecimalHelper
	{
		public static Decimal128 ToDecimal128(this decimal numeral)
		{
			return numeral;
		}

		public static decimal ToDecimal(this Decimal128 numeral)
		{
			return Decimal128.ToDecimal(numeral);
		}
	}
}
