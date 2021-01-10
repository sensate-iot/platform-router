/*
 * Data encapsulation type.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

namespace SensateIoT.Common.Caching.Abstract
{
	public struct KeyValuePair<TKey, TValue> : System.IEquatable<KeyValuePair<TKey, TValue>>
	{
		public TKey Key { get; set; }
		public TValue Value { get; set; }

		public override bool Equals(object obj)
		{
			if(obj is KeyValuePair<TKey, TValue> other) {
				return this.Key.Equals(other.Key) && this.Value.Equals(other.Value);
			}

			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator ==(KeyValuePair<TKey, TValue> left, KeyValuePair<TKey, TValue> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(KeyValuePair<TKey, TValue> left, KeyValuePair<TKey, TValue> right)
		{
			return !(left == right);
		}

		public bool Equals(KeyValuePair<TKey, TValue> other)
		{
			if(!this.Key.Equals(other.Key)) {
				return false;
			}

			if(this.Value == null && other.Value == null) {
				return true;
			}

			return this.Value != null && this.Value.Equals(other.Value);
		}
	}
}