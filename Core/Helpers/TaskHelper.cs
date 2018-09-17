/*
 * Task helpers / short cuts.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SensateService.Helpers
{
	public static class TaskHelper
	{
		public static ConfiguredTaskAwaitable<T> AwaitSafely<T>(this Task<T> tsk, bool otherCtx = false)
		{
			return tsk.ConfigureAwait(otherCtx);
		}

		public static ConfiguredTaskAwaitable AwaitSafely(this Task tsk, bool otherCtx = false)
		{
			return tsk.ConfigureAwait(otherCtx);
		}
	}
}