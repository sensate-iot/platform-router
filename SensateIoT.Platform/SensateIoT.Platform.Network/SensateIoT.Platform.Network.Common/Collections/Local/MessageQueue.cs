/*
 * Queue implementation based on a deque.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Linq;

using SensateIoT.Platform.Network.Common.Collections.Abstract;
using SensateIoT.Platform.Network.Data.Abstract;

namespace SensateIoT.Platform.Network.Common.Collections.Local
{
	public class MessageQueue : Deque<IPlatformMessage>, IMessageQueue
	{
		private const int DefaultCapacity = 1 << 10;

		public MessageQueue() : base(DefaultCapacity)
		{
		}

		public MessageQueue(int capacity) : base(capacity)
		{
		}

		public MessageQueue(IEnumerable<IPlatformMessage> messages) : base(messages)
		{
		}

		public override void Add(IPlatformMessage msg)
		{
			msg.PlatformTimestamp = DateTime.UtcNow;
			base.Add(msg);
		}

		public override void AddRange(IEnumerable<IPlatformMessage> messages)
		{
			var items = messages as IList<IPlatformMessage> ?? messages.ToList();

			foreach(var message in items) {
				message.PlatformTimestamp = DateTime.UtcNow;
			}

			base.AddRange(items);
		}

		public TimeSpan DeltaAge()
		{
			TimeSpan span;
			this.m_lock.Lock();

			try {
				if(this.m_count <= 1) {
					return TimeSpan.Zero;
				}

				var first = this.DoGetByIndex(0);
				var last = this.DoGetByIndex(this.m_count - 1);

				span = last.PlatformTimestamp - first.PlatformTimestamp;
			} finally {
				this.m_lock.Unlock();
			}

			return span;
		}

		public TimeSpan TopMedianDeltaAge()
		{
			TimeSpan span;
			this.m_lock.Lock();

			try {
				if(this.m_count <= 1) {
					return TimeSpan.Zero;
				}

				var top = this.DoGetByIndex(0);
				var middle = this.DoGetByIndex(this.m_count / 2);

				span = middle.PlatformTimestamp - top.PlatformTimestamp;
			} finally {
				this.m_lock.Unlock();
			}

			return span;
		}
	}
}
