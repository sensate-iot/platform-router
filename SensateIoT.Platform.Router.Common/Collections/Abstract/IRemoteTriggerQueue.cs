using System.Threading.Tasks;
using SensateIoT.Platform.Router.Data.Abstract;

namespace SensateIoT.Platform.Router.Common.Collections.Abstract
{
	public interface IRemoteTriggerQueue
	{
		public int Count { get; }

		void EnqueueMessageToTriggerService(IPlatformMessage message);
		void EnqueueMeasurementToTriggerService(IPlatformMessage message);
		Task FlushAsync();
	}
}
