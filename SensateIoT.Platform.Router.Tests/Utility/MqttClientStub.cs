﻿/*
 * MQTT client stub implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using SensateIoT.Platform.Router.Common.Helpers;
using SensateIoT.Platform.Router.Common.MQTT;

namespace SensateIoT.Platform.Router.Tests.Utility
{
	public class MqttClientStub : IInternalMqttClient
	{
		private SpinLockWrapper m_lock;
		private readonly Dictionary<string, int> m_publishCounts;

		public MqttClientStub()
		{
			this.m_lock = new SpinLockWrapper();
			this.m_publishCounts = new Dictionary<string, int>();
		}

		public bool IsConnected => true;

		public Task PublishOnAsync(string topic, string message, bool retain)
		{
			this.m_lock.Lock();

			if(!this.m_publishCounts.TryAdd(topic, 1)) {
				this.m_publishCounts[topic] += 1;
			}

			this.m_lock.Unlock();
			return Task.CompletedTask;
		}

		public int GetPublishCount(string topic)
		{
			this.m_lock.Lock();
			var result = this.m_publishCounts[topic];
			this.m_lock.Unlock();

			return result;
		}
	}
}
