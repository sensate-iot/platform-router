/*
 * Abstraction for local and remote queues.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace SensateIoT.Platform.Network.Common.Collections.Abstract
{
	[PublicAPI]
	[SuppressMessage("ReSharper", "PossibleInterfaceMemberAmbiguity")]
	public interface IQueue<TValue> : IProducerConsumerCollection<TValue>, IReadOnlyCollection<TValue>, IDisposable
	{
		new int Count { get; }
		int Capacity { get; set; }
		new void CopyTo(TValue[] items, int index);

		void AddRangeFront(IEnumerable<TValue> items);
		void AddFront(TValue item);
		void AddRange(IEnumerable<TValue> items);
		void Add(TValue items);

		TValue Dequeue();
		IEnumerable<TValue> DequeueRange(int count);

		TValue this[int index] { get; set; }
		TValue Peek(int index);
		bool Contains(TValue item);

		void Clear();
		void Trim();
	}
}
