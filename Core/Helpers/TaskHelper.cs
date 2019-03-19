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
		public static ConfiguredTaskAwaitable<T> AwaitBackground<T>(this Task<T> tsk, bool background = true)
		{
			return tsk.ConfigureAwait(!background);
		}

		public static ConfiguredTaskAwaitable AwaitBackground(this Task tsk, bool background = true)
		{
			return tsk.ConfigureAwait(!background);
		}
	}
}