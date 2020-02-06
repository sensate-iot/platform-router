/*
 * RESTful HTTP client.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SensateService.Helpers;

namespace SensateService.TriggerHandler.Application
{
	public class RestClient
	{
		private readonly HttpClient m_client;

		public RestClient()
		{
			var client = new HttpClient();

			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Add("User-Agent", "Sensate IoT Trigger Reporter");
			this.m_client = client;
		}

		public async Task<int> GetAsync(string host)
		{
			var result = await this.m_client.GetAsync(host).AwaitBackground();
			return result.StatusCode.ToInt();
		}

		public async Task<int> PostAsync(string host, string body)
		{
			var content = new StringContent(body);

			content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
			var result = await this.m_client.PostAsync(host, content).AwaitBackground();

			return result.StatusCode.ToInt();
		}
	}
}