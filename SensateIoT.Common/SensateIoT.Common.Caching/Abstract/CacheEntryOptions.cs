/*
 * Entry options for various cache types.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

namespace SensateIoT.Common.Caching.Abstract
{
	public class CacheEntryOptions
	{
		private TimeSpan? m_timeout;
		private long? m_size;

		public TimeSpan? Timeout {
			get => this.m_timeout;
			set {
				if(value <= TimeSpan.Zero) {
					throw new ArgumentOutOfRangeException(nameof(this.Timeout), value, "The timeout value must be positive.");
				}

				this.m_timeout = value;
			}
		}

		public long? Size {
			get => this.m_size;
			set {
				if(value < 0) {
					throw new ArgumentOutOfRangeException(nameof(this.Size), value, "Size argument must be greater than 0");
				}

				this.m_size = value;
			}
		}

		public int GetTimeoutMs()
		{
			if(!this.m_timeout.HasValue) {
				return -1;
			}

			return Convert.ToInt32(this.m_timeout.Value.TotalMilliseconds);
		}
	}
}
