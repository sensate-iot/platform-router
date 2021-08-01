/*
 * Queue implementation based on a deque.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Prometheus;
using SensateIoT.Platform.Network.Data.Abstract;

namespace SensateIoT.Platform.Router.Common.Collections.Local
{
	public class MessageQueue : Deque<IPlatformMessage>
	{
		private const int DefaultCapacity = 1 << 10;
		private readonly Gauge m_gauge;

		public MessageQueue() : this(DefaultCapacity)
		{
		}

		public MessageQueue(int capacity) : base(capacity)
		{
			this.m_gauge = Metrics.CreateGauge("router_messages_queued", "Number of messages queued to the router.");
		}

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public MessageQueue(IEnumerable<IPlatformMessage> messages) : base(messages)
		{
			this.m_gauge = Metrics.CreateGauge("router_messages_queued", "Number of messages queued to the router.");
			this.m_gauge.Set(messages.Count());
		}

		public override void Add(IPlatformMessage msg)
		{
			msg.PlatformTimestamp = DateTime.UtcNow;
			base.Add(msg);
			this.m_gauge.Inc();
		}

		public override void AddRange(IEnumerable<IPlatformMessage> messages)
		{
			var items = messages as IList<IPlatformMessage> ?? messages.ToList();

			foreach(var message in items) {
				message.PlatformTimestamp = DateTime.UtcNow;
			}

			base.AddRange(items);
			this.m_gauge.Inc(items.Count);
		}

		public override IEnumerable<IPlatformMessage> DequeueRange(int count)
		{
			var result = base.DequeueRange(count).ToList();

			this.m_gauge.Dec(result.Count);
			return result;
		}
	}
}
