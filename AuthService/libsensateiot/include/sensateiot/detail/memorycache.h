/*
 * Generic in-memory cache.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <algorithm>
#include <utility>
#include <chrono>
#include <shared_mutex>
#include <optional>

#include <tsl/hopscotch_map.h>

#include <boost/uuid/uuid.hpp>
#include <boost/multiprecision/cpp_int.hpp>

#include <sensateiot/stl/referencewrapper.h>
#include <sensateiot/stl/smallvector.h>
#include <sensateiot/util/log.h>
#include <sensateiot/models/objectid.h>

namespace sensateiot::detail
{
	namespace traits
	{
		template<typename T>
		struct IsCallable {
		private:
			typedef char yes;
			typedef std::size_t no;

			struct Fallback {
				void operator()() const
				{
				}
			};

			struct Derived : T, Fallback {
			};

			template<typename U, U>
			struct Check;

			template<typename>
			static constexpr yes test(...)
			{
				return 0;
			}

			template<typename C>
			static constexpr no test(Check<void (Fallback::*)(), &C::operator()> *)
			{
				return 0;
			}

		public:
			static constexpr bool Value = sizeof(test<Derived>(nullptr)) == sizeof(yes);
		};

		template <typename T>
		class TestHashAlgorithm {
			template <typename U>
			static constexpr std::true_type test(typename U::HashType*)
			{
				return {};
			}

			template <typename>
			static constexpr std::false_type test(...)
			{
				return {};
			}

			enum data {
				value = !!decltype(test<T>(nullptr))()
			};

		public:
			static constexpr bool Value = data::value;
		};
	}
	
	namespace algo
	{
		template <typename T, size_t S>
		struct MurmurHash3;
		
		template <typename T>
		struct MurmurHash3<T, 8> {
			typedef std::size_t HashType;
			HashType operator()(const T* array, std::size_t size) const;

			static constexpr HashType seed = 357507768484424009;
			static constexpr HashType m = 0xc6a4a7935bd1e995;
			static constexpr int r = 47;
		};

		template <typename T>
		struct MurmurHash3<T, 4> {
			typedef std::size_t HashType;
			HashType operator()(const T* array, std::size_t size) const;

			static constexpr HashType seed = 125805157;
			static constexpr HashType m = 0x5bd1e995;
			static constexpr int r = 24;
		};

		template <typename T>
		struct FnvHash {
			typedef std::uint64_t HashType;
			HashType operator()(const T* array, std::size_t size) const;

		private:
			static constexpr HashType Seed = 0xcbf29ce484222325;
			static constexpr HashType Prime = 0x100000001b3;
		};
	}

	struct HashAlgorithm_base {
		typedef std::size_t HashType;
	};

	template <typename T, std::size_t size = sizeof(std::size_t)>
	struct [[maybe_unused]] HashAlgorithm : public HashAlgorithm_base {

		HashType operator()(const T& value) const
		{
			return this->m_hash(value);
		}

	private:
		std::hash<T> m_hash;
	};

	template <std::size_t S>
	struct [[maybe_unused]] HashAlgorithm<std::string, S> : public HashAlgorithm_base {
		typedef typename algo::MurmurHash3<std::uint8_t, S>::HashType HashType;

		HashType operator()(const std::string& value) const
		{
			const auto* data = reinterpret_cast<const std::uint8_t*>(value.data());
			return this->m_algo(data, value.size());
		}

	private:
		algo::MurmurHash3<std::uint8_t, S> m_algo;
	};

	template <std::size_t S>
	struct [[maybe_unused]] HashAlgorithm<models::ObjectId, S> : public HashAlgorithm_base {
		typedef algo::FnvHash<std::uint8_t>::HashType HashType;

		HashType operator()(const models::ObjectId& value) const
		{
			stl::SmallVector<std::uint8_t, models::ObjectId::ObjectIdSize> v;

			boost::multiprecision::export_bits(value.Value(), std::back_inserter(v), 8);
			return this->m_algo(v.data(), v.size());
		}

	private:
		algo::FnvHash<std::uint8_t> m_algo;
	};

	template <std::size_t S>
	struct [[maybe_unused]] HashAlgorithm<boost::uuids::uuid, S> : public HashAlgorithm_base {
		typedef algo::FnvHash<std::uint8_t>::HashType HashType;

		HashType operator()(const boost::uuids::uuid& value) const
		{
			return this->m_algo(value.data, value.size());
		}

	private:
		algo::FnvHash<std::uint8_t> m_algo;
	};
	
	struct MemoryCacheEntryOptions {
		[[nodiscard]] std::optional<std::chrono::high_resolution_clock::duration> GetTimeout() const
		{
			return m_timeout;
		}

		[[nodiscard]] std::optional<std::size_t> GetSize() const
		{
			return m_size;
		}
		
		void SetTimeout(const std::optional<std::chrono::high_resolution_clock::duration>& m_timeout)
		{
			this->m_timeout = m_timeout;
		}

		void SetSize(const std::optional<std::size_t>& m_size)
		{
			this->m_size = m_size;
		}
		
	private:
		std::optional<std::chrono::high_resolution_clock::duration> m_timeout;
		std::optional<std::size_t> m_size;
	};
	
	template <typename V>
	struct MemoryCacheEntry {
		typedef V ValueType;

		explicit MemoryCacheEntry() : Value(), Timeout(), Size(0)
		{
		}
		
		explicit MemoryCacheEntry(const V& value) : Value(value), Timeout(), Size(0)
		{
		}
		
		explicit MemoryCacheEntry(V&& value) : Value(std::forward<ValueType>(value)), Timeout(), Size(0)
		{
		}

		ValueType Value;
		std::chrono::high_resolution_clock::duration Timeout;
		std::chrono::high_resolution_clock::time_point Timestamp;
		std::size_t Size;
	};

	template <typename K,
			  typename V,
			  typename Equal,
			  typename Hash,
			  typename A
	>
	class MemoryCache_Helper {
		static constexpr bool IsHashAlgorithmTypeValid()
		{
			return traits::IsCallable<Hash>::Value && traits::TestHashAlgorithm<Hash>::Value;
		}

		static constexpr bool IsEqualityTypeValid()
		{
			return traits::IsCallable<Equal>::Value;
		}

		static_assert(IsHashAlgorithmTypeValid(), "The hash algorithm should be callable and define the HashType type.");
		static_assert(IsEqualityTypeValid(), "The equality algorithm should be callable.");
		
		static_assert(std::is_move_constructible_v<K>, "K should be move constructable!");
		static_assert(std::is_move_constructible_v<V>, "V should be move constructable!");

		static_assert(std::is_copy_constructible_v<K>, "K should be copy constructable!");
		static_assert(std::is_copy_constructible_v<V>, "V should be copy constructable!");

		static_assert(std::is_move_assignable_v<K>, "K should be move assignable!");
		static_assert(std::is_move_assignable_v<V>, "V should be move assignable!");

		static_assert(std::is_copy_assignable_v<K>, "K should be copy assignable!");
		static_assert(std::is_copy_assignable_v<V>, "V should be copy assignable!");
	};
	
	template <typename K,
			  typename V,
			  typename Equal = std::equal_to<K>,
			  typename Hash = HashAlgorithm<K>,
			  typename A = std::allocator<std::pair<K, MemoryCacheEntry<V>>>
	>
	class MemoryCache : private MemoryCache_Helper<K, V, Equal, Hash, A> {
		static constexpr auto IsDefaultConstructible = std::is_default_constructible<V>::value;

	public:
		typedef K KeyType;
		typedef V ValueType;
		typedef A AllocatorType;
		typedef Equal KeyComparatorType;
		typedef Hash HashFunctionType;
		typedef std::pair<KeyType, ValueType> EntryType;
		typedef std::chrono::high_resolution_clock ClockType;

		explicit MemoryCache();
		explicit MemoryCache(ClockType::duration timeout);
		explicit MemoryCache(std::size_t capacity);
		explicit MemoryCache(ClockType::duration timeout, std::size_t capacity);

		MemoryCache(const MemoryCache& other);
		MemoryCache(MemoryCache&& other) noexcept;

		MemoryCache& operator=(const MemoryCache& rhs);
		MemoryCache& operator=(MemoryCache&& rhs) noexcept;

		virtual ~MemoryCache();

		[[nodiscard]] std::optional<ValueType> TryGetValue(const KeyType& key, ClockType::time_point timestamp) const;
		[[nodiscard]] std::optional<ValueType> TryGetValue(const KeyType& key) const;
		[[nodiscard]] bool Contains(const KeyType& key) const;

		template <bool default_constructible = IsDefaultConstructible, std::enable_if_t<default_constructible, int> = 0>
		ValueType& operator[](const KeyType& key);
		const ValueType& operator[](const KeyType& key) const;

		void Add(const EntryType& entry, const MemoryCacheEntryOptions& options);
		void Add(EntryType&& entry, const MemoryCacheEntryOptions& options);
		void AddRange(const std::vector<EntryType>& entries, MemoryCacheEntryOptions options);
		void AddRange(std::vector<EntryType>&& entries, MemoryCacheEntryOptions options);
		void AddOrUpdate(const EntryType& entry, MemoryCacheEntryOptions options);
		void AddOrUpdateRange(const std::vector<EntryType>& entries, const MemoryCacheEntryOptions& options);
		void AddOrUpdateRange(std::vector<EntryType>&& entries, const MemoryCacheEntryOptions& options);

		void Update(const EntryType& entry, const MemoryCacheEntryOptions& options);
		void Update(EntryType&& entry, const MemoryCacheEntryOptions& options);
		void UpdateRange(const std::vector<EntryType>& entries, const MemoryCacheEntryOptions& options);
		void UpdateRange(std::vector<EntryType>&& entries, const MemoryCacheEntryOptions& options);
		
		void Remove(const KeyType& key);
		void Clear();

		void Cleanup();
		bool Cleanup(ClockType::duration period);

		[[nodiscard]] std::size_t Count() const;
		[[nodiscard]] std::size_t Size() const;

		void SetCapacity(std::size_t capacity);
		[[nodiscard]] std::size_t Capacity() const;

		void SetTimeout(ClockType::duration timeout);
		[[nodiscard]] ClockType::duration Timeout() const;

	protected:
		static constexpr int NeighborhoodSize = 30;
		static constexpr bool StoreHash = NeighborhoodSize <= 30;
		typedef MemoryCacheEntry<ValueType> InternalEntryType;
		
		mutable std::shared_mutex m_dataLock;
		tsl::hopscotch_map<KeyType, InternalEntryType, HashFunctionType, KeyComparatorType, AllocatorType, NeighborhoodSize, StoreHash> m_data;

		void InternalAddLocked(EntryType&& entry, const MemoryCacheEntryOptions& options);
		void InternalAddOrUpdateLocked(EntryType&& entry, const MemoryCacheEntryOptions& options, bool updateExisting);
		void InternalAddLocked(const EntryType& entry, const MemoryCacheEntryOptions& options);
		void InternalAddOrUpdateLocked(const EntryType& entry, const MemoryCacheEntryOptions& options, bool updateExisting);
		void InternalUpdateLocked(const EntryType& value, InternalEntryType& entry, const MemoryCacheEntryOptions& options);
		void InternalUpdateLocked(EntryType&& value, InternalEntryType& entry, const MemoryCacheEntryOptions& options);

		virtual void Move(MemoryCache&& other);
		virtual void Copy(const MemoryCache& other);

	private:
		static constexpr int CleanupBatchSize = 100;
		static constexpr int DefaultTimeoutMinutes = 5;

		std::optional<std::size_t> m_capacity;
		std::optional<std::size_t> m_size;
		ClockType::duration m_timeout;

		stl::ReferenceWrapper<InternalEntryType> ValidateCacheKey(const KeyType& key, bool throwIfExists);
		void ValidateCacheEntryOptions(const MemoryCacheEntryOptions& options);
		bool ValidateAndUpdateCapacity(const InternalEntryType& entry);
		bool ValidateAndUpdateCapacityForUpdate(const InternalEntryType& entry, std::size_t newEntrySize);
		bool RawCleanupLocked();
	};
}

#include "memorycache.hpp"
