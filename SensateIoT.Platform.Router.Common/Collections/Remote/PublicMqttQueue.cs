/*
 * Public MQTT queue implementation.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SensateIoT.Platform.Router.Common.Collections.Abstract;
using SensateIoT.Platform.Router.Common.Collections.Local;
using SensateIoT.Platform.Router.Common.MQTT;

namespace SensateIoT.Platform.Router.Common.Collections.Remote
{
	internal class RemoteQueueMessage
	{
		internal string Data { get; set; }
		internal string Target { get; set; }
	}

	public class PublicMqttQueue : IPublicRemoteQueue
	{
		private readonly IQueue<RemoteQueueMessage> m_queue;
		private readonly IPublicMqttClient m_client;

		private const int DequeueCount = 1000;
		private const int MaxIterations = 5;

		public PublicMqttQueue(IPublicMqttClient client)
		{
			this.m_client = client;
			this.m_queue = new Deque<RemoteQueueMessage>(1024);
		}

		public int QueueLength => this.m_queue.Count;

		public void Enqueue(string data, string target)
		{
			this.m_queue.Add(new RemoteQueueMessage {
				Target = target,
				Data = data
			});
		}

		public async Task FlushQueueAsync()
		{
			var tasks = new List<Task>();
			int count;
			var iteration = 0;

			do {
				var messages = this.m_queue.DequeueRange(DequeueCount).ToList();

				count = messages.Count;

				if(count == 0) {
					break;
				}

				foreach(var msg in messages) {
					tasks.Add(this.m_client.PublishOnAsync(msg.Target, msg.Data, false));
				}

				await Task.WhenAll(tasks).ConfigureAwait(false);
				tasks.Clear();

				iteration++;
			} while(count > 0 && iteration < MaxIterations);
		}
	}
}
