using System;
using System.Threading.Tasks;

using Google.Protobuf;

namespace SensateService.Infrastructure.Events
{
	public delegate Task OnDataAuthorizedEvent(object sender, DataAuthorizedEventArgs e);

	public class DataAuthorizedEventArgs : EventArgs
	{
		public IMessage Data { get; set; }
	}
}
