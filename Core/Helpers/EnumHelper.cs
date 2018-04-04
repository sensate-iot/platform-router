/*
 * Enumeration extension methods.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;

namespace SensateService.Helpers
{
	public static class EnumHelper
	{
		public static int ToInt<T>(this T source) where T : IConvertible
		{
			if(!typeof(T).IsEnum)
				throw new ArgumentException("Type argument must be an enumerated type!");

			return (int) (IConvertible)source;
		}
	}

	public class Enum<T> where T : struct, IConvertible
	{
		public static int Count
		{
			get {
				if(!typeof(T).IsEnum)
					throw new ArgumentException("Type argument must be an enumerated type!");

				return Enum.GetNames(typeof(T)).Length;
			}
		}
	}
}
