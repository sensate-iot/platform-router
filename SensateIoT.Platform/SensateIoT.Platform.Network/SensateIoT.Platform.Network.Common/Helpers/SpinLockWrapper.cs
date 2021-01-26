/*
 * Wrapper class for SpinLocks.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System.Threading;

namespace SensateIoT.Platform.Network.Common.Helpers
{
	public struct SpinLockWrapper
	{
		private SpinLock _lock;

		public bool IsLocked { get; private set; }

		public SpinLockWrapper(bool tracking = false)
		{
			this._lock = new SpinLock(tracking);
			this.IsLocked = false;
		}

		public void Lock()
		{
			var locked = false;

			this._lock.Enter(ref locked);
			this.IsLocked = locked;

			if(!locked) {
				throw new SynchronizationLockException("Unable to obtain spin lock!");
			}
		}

		public void Unlock()
		{
			if(!this.IsLocked)
				return;

			this.IsLocked = false;
			this._lock.Exit();
		}
	}
}
