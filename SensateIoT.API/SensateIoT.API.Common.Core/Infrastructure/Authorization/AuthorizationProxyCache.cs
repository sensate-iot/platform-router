/*
 * Cached proxy for the authorization service.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SensateIoT.API.Common.Core.Helpers;

namespace SensateIoT.API.Common.Core.Infrastructure.Authorization
{
	public class AuthorizationProxyCache : IMeasurementAuthorizationProxyCache, IMessageAuthorizationProxyCache
	{
		private IList<JToken> m_objects;
		private SpinLockWrapper m_lock;
		private readonly HttpClient m_client;

		private const int ListCapacity = 100000;
		private const int PartitionSize = 100;

		public AuthorizationProxyCache()
		{
			this.m_objects = new List<JToken>(ListCapacity);
			this.m_lock = new SpinLockWrapper();
			var client = new HttpClient();

			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Add("User-Agent", "Sensate IoT Auth Proxy");
			this.m_client = client;
		}

		public void AddMessage(string data)
		{
			var obj = JObject.Parse(data);

			this.m_lock.Lock();
			this.m_objects.Add(obj);
			this.m_lock.Unlock();
		}

		public void AddMessages(string data)
		{
			var ary = JArray.Parse(data);

			this.m_lock.Lock();
			foreach(var value in ary) {
				this.m_objects.Add(value);
			}
			this.m_lock.Unlock();
		}

		public async Task<long> ProcessAsync(string remote)
		{
			List<IList<JToken>> partitions;
			IList<JToken> data;

			this.m_lock.Lock();
			data = this.m_objects;
			this.m_objects = new List<JToken>(ListCapacity);
			this.m_lock.Unlock();

			if(data.Count <= 0) {
				return 0L;
			}

			partitions = data.Partition(PartitionSize).ToList();
			var count = 0L;
			var httpTasks = new Task<HttpResponseMessage>[partitions.Count];

			Parallel.For(0, partitions.Count, (idx) => {
				var json = JsonConvert.SerializeObject(partitions[idx]);
				var content = new StringContent(json);

				content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
				httpTasks[idx] = this.m_client.PostAsync(remote, content);
			});

			var responses = await Task.WhenAll(httpTasks).AwaitBackground();

			foreach(var response in responses) {
				if(!response.IsSuccessStatusCode) {
					continue;
				}

				var rcv = await response.Content.ReadAsStringAsync().AwaitBackground();
				var obj = JObject.Parse(rcv);
				var jcount = obj.GetValue("count");

				if(jcount == null) {
					continue;
				}

				count += jcount.ToObject<int>();
			}

			return count;
		}
	}
}