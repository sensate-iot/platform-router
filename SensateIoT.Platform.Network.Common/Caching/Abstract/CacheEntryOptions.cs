/*
 * Entry options for various cache types.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;

namespace SensateIoT.Platform.Network.Common.Caching.Abstract
{
	public class CacheEntryOptions
	{
		private TimeSpan? m_timeout;
		private long? m_size;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Reviewed")]
		public TimeSpan? Timeout {
			get => this.m_timeout;
			set {
				if(value <= TimeSpan.Zero) {
					throw new ArgumentOutOfRangeException(nameof(this.Timeout), value, "The timeout value must be positive.");
				}

				this.m_timeout = value;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Reviewed")]
		public long? Size {
			get => this.m_size;
			set {
				if(value < 0) {
					throw new ArgumentOutOfRangeException(nameof(this.Size), value, "Size argument must be greater than 0");
				}

				this.m_size = value;
			}
		}
	}
}
