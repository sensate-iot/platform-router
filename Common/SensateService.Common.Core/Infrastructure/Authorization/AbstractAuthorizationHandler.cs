/*
 * Abstract authorization handler.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SensateService.Common.Data.Dto.Authorization;
using SensateService.Helpers;

namespace SensateService.Infrastructure.Authorization
{
	public abstract class AbstractAuthorizationHandler<TData> where TData : class
	{
		protected SpinLockWrapper m_lock;
		protected List<TData> m_messages;

		protected const int SecretSubStringOffset = 3;
		protected const int SecretSubStringStart = 1;
		protected const int PartitionSize = 10000;

		protected AbstractAuthorizationHandler()
		{
			this.m_lock = new SpinLockWrapper();
			this.m_messages = new List<TData>();
		}

		public void AddMessage(TData data)
		{
			this.m_lock.Lock();

			try {
				this.m_messages.Add(data);
			} finally {
				this.m_lock.Unlock();
			}
		}

		public void AddMessages(IEnumerable<TData> messages)
		{
			this.m_lock.Lock();

			try {
				this.m_messages.AddRange(messages);
			} finally {
				this.m_lock.Unlock();
			}
		}

		protected abstract bool AuthorizeMessage(TData data, Sensor sensor);
		public abstract Task ProcessAsync();

		protected static byte[] HexToByteArray(string hex)
		{
			var num = hex.Length;
			var bytes = new byte[num / 2];

			for(var i = 0; i < num; i += 2) {
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			}

			return bytes;
		}

		protected static bool CompareHashes(ReadOnlySpan<byte> h1, ReadOnlySpan<byte> h2)
		{
			return h1.SequenceEqual(h2);
		}
	}
}