/*
 * Generic in-memory cache.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

namespace sensateiot::detail
{
	namespace algo
	{
		template<typename T>
		inline typename MurmurHash3<T, 8>::HashType MurmurHash3<T, 8>::operator()(const T* array, std::size_t size) const
		{
			uint64_t h = seed ^ (size * m);

			const uint64_t* data = (const uint64_t*)array;
			const uint64_t* end = data + (size / sizeof(uint64_t));

			while(data != end) {
				uint64_t k = *data++;

				k *= m;
				k ^= k >> r;
				k *= m;

				h ^= k;
				h *= m;
			}

			const auto* data2 = (const std::uint8_t*)data;

			switch(size & 7) {
			case 7:
				h ^= uint64_t(data2[6]) << 48;
				
			case 6:
				h ^= uint64_t(data2[5]) << 40;
				
			case 5:
				h ^= uint64_t(data2[4]) << 32;
				
			case 4:
				h ^= uint64_t(data2[3]) << 24;
				
			case 3:
				h ^= uint64_t(data2[2]) << 16;
				
			case 2:
				h ^= uint64_t(data2[1]) << 8;
				
			case 1:
				h ^= uint64_t(data2[0]);
				h *= m;

			default:
				break;
			}

			h ^= h >> r;
			h *= m;
			h ^= h >> r;

			return h;
		}

		template<typename T>
		inline typename MurmurHash3<T, 4>::HashType MurmurHash3<T, 4>::operator()(const T* array, std::size_t size) const
		{
			uint32_t h = seed ^ size;
			const auto* data = (const std::uint8_t*)array;

			while(size >= 4) {
				uint32_t k = *(uint32_t*)data;

				k *= m;
				k ^= k >> r;
				k *= m;

				h *= m;
				h ^= k;

				data += 4;
				size -= 4;
			}

			switch(size) {
			case 3:
				h ^= data[2] << 16;
				
			case 2:
				h ^= data[1] << 8;
				
			case 1:
				h ^= data[0];
				h *= m;

			default:
				break;
			};

			h ^= h >> 13;
			h *= m;
			h ^= h >> 15;

			return h;
		}

		template<typename T>
		inline typename FnvHash<T>::HashType FnvHash<T>::operator()(const T* array, std::size_t size) const
		{
			const auto* bytes = static_cast<const std::uint8_t*>(array);
			auto hash = Seed;

			if constexpr(!std::is_same<T, std::uint8_t>::value) {
				size *= sizeof(T);
			}

			for(std::size_t idx{ 0 }; idx < size; idx++) {
				hash ^= bytes[idx];
				hash *= Prime;
			}

			return hash;
		}
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	inline MemoryCache<K, V, Equal, Hash, A>::MemoryCache() : m_timeout(std::chrono::minutes(DefaultTimeoutMinutes))
	{
	}

	template<typename K, typename V, typename Equal, typename Hash, typename A>
	inline MemoryCache<K, V, Equal, Hash, A>::MemoryCache(ClockType::duration timeout) : m_timeout(timeout)
	{
	}

	template<typename K, typename V, typename Equal, typename Hash, typename A>
	inline MemoryCache<K, V, Equal, Hash, A>::MemoryCache(std::size_t capacity) :
		m_capacity(capacity), m_size(0), m_timeout(std::chrono::minutes(DefaultTimeoutMinutes))
	{
	}

	template<typename K, typename V, typename Equal, typename Hash, typename A>
	inline MemoryCache<K, V, Equal, Hash, A>::MemoryCache(ClockType::duration timeout, std::size_t capacity) :
		m_capacity(capacity), m_size(0), m_timeout(timeout)
	{
	}

	template<typename K, typename V, typename Equal, typename Hash, typename A>
	inline MemoryCache<K, V, Equal, Hash, A>::MemoryCache(const MemoryCache& other)
	{
		this->Copy(other);
	}

	template<typename K, typename V, typename Equal, typename Hash, typename A>
	inline MemoryCache<K, V, Equal, Hash, A>::MemoryCache(MemoryCache&& other) noexcept :
		m_data(std::move(other.m_data)), m_capacity(std::move(other.m_capacity)),
		m_size(std::move(other.m_size)), m_timeout(std::move(other.m_timeout))
	{
	}

	template<typename K, typename V, typename Equal, typename Hash, typename A>
	MemoryCache<K, V, Equal, Hash, A>& MemoryCache<K, V, Equal, Hash, A>::operator=(const MemoryCache& rhs)
	{
		if(this == &rhs) {
			return *this;
		}

		this->Copy(rhs);
		return *this;
	}

	template<typename K, typename V, typename Equal, typename Hash, typename A>
	MemoryCache<K, V, Equal, Hash, A>& MemoryCache<K, V, Equal, Hash, A>::operator=(MemoryCache&& rhs) noexcept
	{
		if(this == &rhs) {
			return *this;
		}

		this->Move(std::move(rhs));
		return *this;
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	MemoryCache<K, V, Equal, Hash, A>::~MemoryCache()
	{
		this->Clear();
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	std::optional<V> MemoryCache<K, V, Equal, Hash, A>::TryGetValue(const KeyType& key) const
	{
		return this->TryGetValue(key, ClockType::now());
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	std::optional<V> MemoryCache<K, V, Equal, Hash, A>::TryGetValue(const KeyType& key, ClockType::time_point timestamp) const
	{
		std::shared_lock lock(this->m_dataLock);
		auto iter = this->m_data.find(key);

		if(iter == this->m_data.cend()) {
			return {};
		}

		auto entry = iter.value();
		ClockType::duration age = timestamp - entry.Timestamp;

		if(age.count() >= entry.Timeout.count()) {
			return {};
		}

		return std::make_optional(std::move(entry.Value));
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	bool MemoryCache<K, V, Equal, Hash, A>::Contains(const KeyType& key) const
	{
		auto rv = this->TryGetValue(key);
		return rv.has_value();
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	template <bool default_constructible, std::enable_if_t<default_constructible, int>>
	typename MemoryCache<K, V, Equal, Hash, A>::ValueType& MemoryCache<K, V, Equal, Hash, A>::operator[](const KeyType& key)
	{
		std::unique_lock lock(this->m_dataLock);
		auto result = this->m_data.find(key);
		auto now = ClockType::now();

		if(result == this->m_data.end()) {
			InternalEntryType e;

			e.Timeout = this->m_timeout;
			e.Timestamp = now;

			this->m_data.emplace(key, std::move(e));
		}

		auto& ref = this->m_data.at(key);
		ref.Timestamp = now;

		return ref.Value;
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	const typename MemoryCache<K, V, Equal, Hash, A>::ValueType& MemoryCache<K, V, Equal, Hash, A>::operator[](const KeyType& key) const
	{
		std::shared_lock lock(this->m_dataLock);

		if(!this->m_data.contains(key)) {
			throw std::out_of_range("Unable to find key in memory cache!");
		}

		auto& entry = this->m_data.at(key);
		return entry.Value;
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	void MemoryCache<K, V, Equal, Hash, A>::Add(EntryType&& entry, const MemoryCacheEntryOptions& options)
	{
		std::unique_lock lock(this->m_dataLock);

		this->ValidateCacheEntryOptions(options);
		this->InternalAddOrUpdateLocked(std::forward<EntryType>(entry), options, false);
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	void MemoryCache<K, V, Equal, Hash, A>::AddRange(const std::vector<EntryType>& entries, MemoryCacheEntryOptions options)
	{
		std::unique_lock lock(this->m_dataLock);
		this->ValidateCacheEntryOptions(options);

		for(auto& entry : entries) {
			this->InternalAddOrUpdateLocked(entry, options, false);
		}
	}
	
	template <typename K, typename V, typename Equal, typename Hash, typename A>
	void MemoryCache<K, V, Equal, Hash, A>::AddRange(std::vector<EntryType>&& entries, MemoryCacheEntryOptions options)
	{
		std::unique_lock lock(this->m_dataLock);
		this->ValidateCacheEntryOptions(options);

		for(auto& entry : entries) {
			this->InternalAddOrUpdateLocked(std::move(entry), options, false);
		}
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	void MemoryCache<K, V, Equal, Hash, A>::Add(const EntryType& entry, const MemoryCacheEntryOptions& options)
	{
		std::unique_lock lock(this->m_dataLock);

		this->ValidateCacheEntryOptions(options);
		this->InternalAddOrUpdateLocked(entry, options, false);
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	void MemoryCache<K, V, Equal, Hash, A>::AddOrUpdate(const EntryType& entry, MemoryCacheEntryOptions options)
	{
		std::unique_lock lock(this->m_dataLock);

		this->ValidateCacheEntryOptions(options);
		this->InternalAddOrUpdateLocked(entry, options, true);
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	void MemoryCache<K, V, Equal, Hash, A>::AddOrUpdateRange(const std::vector<EntryType>& entries, const MemoryCacheEntryOptions& options)
	{
		std::unique_lock lock(this->m_dataLock);
		this->ValidateCacheEntryOptions(options);

		for(auto entry : entries) {
			this->InternalAddOrUpdateLocked(entry, options, true);
		}
	}
	
	template <typename K, typename V, typename Equal, typename Hash, typename A>
	void MemoryCache<K, V, Equal, Hash, A>::AddOrUpdateRange(std::vector<EntryType>&& entries, const MemoryCacheEntryOptions& options)
	{
		std::unique_lock lock(this->m_dataLock);
		this->ValidateCacheEntryOptions(options);

		for(auto entry : entries) {
			this->InternalAddOrUpdateLocked(std::move(entry), options, true);
		}
	}
	
	template<typename K, typename V, typename Equal, typename Hash, typename A>
	inline void MemoryCache<K, V, Equal, Hash, A>::Update(EntryType&& entry, const MemoryCacheEntryOptions& options)
	{
		std::unique_lock lock(this->m_dataLock);

		this->ValidateCacheEntryOptions(options);
		auto result = this->ValidateCacheKey(entry.first, false);

		if(result) {
			this->InternalUpdateLocked(std::forward<EntryType>(entry), result.get(), options);
		} else {
			throw std::out_of_range("Unable to update non existing cache key!");
		}
	}

	template<typename K, typename V, typename Equal, typename Hash, typename A>
	inline void MemoryCache<K, V, Equal, Hash, A>::Update(const EntryType& entry, const MemoryCacheEntryOptions& options)
	{
		std::unique_lock lock(this->m_dataLock);

		this->ValidateCacheEntryOptions(options);
		auto result = this->ValidateCacheKey(entry.first, false);

		if(result) {
			this->InternalUpdateLocked(entry, result.get(), options);
		} else {
			throw std::out_of_range("Unable to update non existing cache key!");
		}
	}
	
	template<typename K, typename V, typename Equal, typename Hash, typename A>
	inline void MemoryCache<K, V, Equal, Hash, A>::UpdateRange(const std::vector<EntryType>& entries, const MemoryCacheEntryOptions& options)
	{
		std::unique_lock lock(this->m_dataLock);
		this->ValidateCacheEntryOptions(options);

		for (const auto& entry : entries) {
			auto result = this->ValidateCacheKey(entry.first, false);

			if(result) {
				this->InternalUpdateLocked(entry, result.get(), options);
			} else {
				throw std::out_of_range("Unable to update non-existing cache key");
			}
		}
	}
	
	template<typename K, typename V, typename Equal, typename Hash, typename A>
	inline void MemoryCache<K, V, Equal, Hash, A>::UpdateRange(std::vector<EntryType>&& entries, const MemoryCacheEntryOptions& options)
	{
		std::unique_lock lock(this->m_dataLock);
		this->ValidateCacheEntryOptions(options);

		for (auto&& entry : entries) {
			auto result = this->ValidateCacheKey(entry.first, false);

			if(result) {
				this->InternalUpdateLocked(std::forward<EntryType>(entry), result.get(), options);
			} else {
				throw std::out_of_range("Unable to update non-existing cache key");
			}
		}
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	void MemoryCache<K, V, Equal, Hash, A>::Remove(const KeyType& key)
	{
		std::unique_lock lock(this->m_dataLock);
		auto iter = this->m_data.find(key);

		if(iter == this->m_data.end()) {
			throw std::out_of_range("Unable to remove non-existing key");
		}

		if(this->m_size.has_value()) {
			auto& value = iter.value();
			*(this->m_size) -= value.Size;
		}

		this->m_data.erase(iter);
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	void MemoryCache<K, V, Equal, Hash, A>::Clear()
	{
		std::unique_lock lock(this->m_dataLock);
		
		if(this->m_size.has_value()) {
			this->m_size.emplace(0);
		}

		this->m_data.clear();
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	void MemoryCache<K, V, Equal, Hash, A>::Cleanup()
	{
		bool done;
		std::unique_lock lock(this->m_dataLock);

		do {
			done = this->RawCleanupLocked();
		} while(!done);
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	bool MemoryCache<K, V, Equal, Hash, A>::Cleanup(ClockType::duration period)
	{
		std::unique_lock lock(this->m_dataLock);

		bool done;
		auto start = ClockType::now();

		do {
			done = this->RawCleanupLocked();
			ClockType::duration duration = ClockType::now() - start;

			if(duration > period) {
				break;
			}
		} while(!done);

		return done;
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	std::size_t MemoryCache<K, V, Equal, Hash, A>::Count() const
	{
		std::shared_lock lock(this->m_dataLock);
		return this->m_data.size();
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	std::size_t MemoryCache<K, V, Equal, Hash, A>::Size() const
	{
		std::shared_lock lock(this->m_dataLock);
		std::size_t rv = 0;

		if(this->m_size.has_value()) {
			rv = this->m_size.value();
		}

		return rv;
	}

	template<typename K, typename V, typename Equal, typename Hash, typename A>
	inline void MemoryCache<K, V, Equal, Hash, A>::SetCapacity(std::size_t capacity)
	{
		std::unique_lock lock(this->m_dataLock);

		if(this->m_capacity.has_value()) {
			this->m_capacity.emplace(std::move(capacity));
		} else {
			throw std::runtime_error("Unable update capacity: cache is created as uncapped cache");
		}
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	std::size_t MemoryCache<K, V, Equal, Hash, A>::Capacity() const
	{
		std::shared_lock lock(this->m_dataLock);
		std::size_t rv = 0;

		if(this->m_capacity.has_value()) {
			rv = this->m_capacity.value();
		}

		return rv;
	}

	template<typename K, typename V, typename Equal, typename Hash, typename A>
	inline void MemoryCache<K, V, Equal, Hash, A>::SetTimeout(ClockType::duration timeout)
	{
		std::unique_lock lock(this->m_dataLock);
		this->m_timeout = std::move(timeout);
	}

	template<typename K, typename V, typename Equal, typename Hash, typename A>
	inline std::chrono::high_resolution_clock::duration MemoryCache<K, V, Equal, Hash, A>::Timeout() const
	{
		std::shared_lock lock(this->m_dataLock);
		return this->m_timeout;
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	void MemoryCache<K, V, Equal, Hash, A>::InternalAddLocked(EntryType&& entry, const MemoryCacheEntryOptions& options)
	{
		InternalEntryType e(std::move(entry.second));

		if(options.GetTimeout().has_value()) {
			e.Timeout = options.GetTimeout().value();
		} else {
			e.Timeout = this->m_timeout;
		}

		e.Timestamp = ClockType::now();

		if(options.GetSize().has_value()) {
			e.Size = options.GetSize().value();
		}

		if(!this->ValidateAndUpdateCapacity(e)) {
			throw std::overflow_error("Unable to add entry to memory cache: entry to big.");
		}

		this->m_data.emplace(std::move(entry.first), std::move(e));
	}
	
	template <typename K, typename V, typename Equal, typename Hash, typename A>
	void MemoryCache<K, V, Equal, Hash, A>::InternalUpdateLocked(EntryType&& value, InternalEntryType& entry, const MemoryCacheEntryOptions& options)
	{
		std::size_t size = 0;

		if(options.GetSize().has_value()) {
			size = options.GetSize().value();
		}
		
		if(!this->ValidateAndUpdateCapacityForUpdate(entry, size)) {
			throw std::overflow_error("Unable to update: entry to big!");
		}

		entry.Value = std::move(value.second);
		entry.Timestamp = ClockType::now();

		if(options.GetTimeout().has_value()) {
			entry.Timeout = options.GetTimeout().value();
		} else {
			entry.Timeout = this->m_timeout;
		}

		if(options.GetSize().has_value()) {
			entry.Size = options.GetSize().value();
		}
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	void MemoryCache<K, V, Equal, Hash, A>::InternalUpdateLocked(const EntryType& value, InternalEntryType& entry, const MemoryCacheEntryOptions& options)
	{
		std::size_t size = 0;

		if(options.GetSize().has_value()) {
			size = options.GetSize().value();
		}
		
		if(!this->ValidateAndUpdateCapacityForUpdate(entry, size)) {
			throw std::overflow_error("Unable to update: entry to big!");
		}

		entry.Value = value.second;
		entry.Timestamp = ClockType::now();

		if(options.GetTimeout().has_value()) {
			entry.Timeout = options.GetTimeout().value();
		} else {
			entry.Timeout = this->m_timeout;
		}

		if(options.GetSize().has_value()) {
			entry.Size = options.GetSize().value();
		}
	}

	template<typename K, typename V, typename Equal, typename Hash, typename A>
	inline void MemoryCache<K, V, Equal, Hash, A>::Move(MemoryCache&& other)
	{
		std::unique_lock ownLock(this->m_dataLock, std::defer_lock);
		std::unique_lock otherLock(other.m_dataLock, std::defer_lock);

		std::lock(ownLock, otherLock);
		this->m_data = std::move(other.m_data);
		this->m_timeout = std::move(other.m_timeout);
		this->m_capacity = std::move(other.m_capacity);
		this->m_size = std::move(other.m_size);
	}

	template<typename K, typename V, typename Equal, typename Hash, typename A>
	inline void MemoryCache<K, V, Equal, Hash, A>::Copy(const MemoryCache& other)
	{
		std::unique_lock ownLock(this->m_dataLock, std::defer_lock);
		std::unique_lock otherLock(other.m_dataLock, std::defer_lock);

		std::lock(ownLock, otherLock);
		this->m_data = other.m_data;
		this->m_timeout = other.m_timeout;
		this->m_capacity = other.m_capacity;
		this->m_size = other.m_size;
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	void MemoryCache<K, V, Equal, Hash, A>::InternalAddOrUpdateLocked(EntryType&& entry, const MemoryCacheEntryOptions& options, bool updateExisting)
	{
		auto result = this->ValidateCacheKey(entry.first, !updateExisting);

		if(result) {
			this->InternalUpdateLocked(std::forward<EntryType>(entry), *result, options);
		} else {
			this->InternalAddLocked(std::forward<EntryType>(entry), options);
		}
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	void MemoryCache<K, V, Equal, Hash, A>::InternalAddLocked(const EntryType& entry, const MemoryCacheEntryOptions& options)
	{
		InternalEntryType e(entry.second);

		e.Timestamp = ClockType::now();

		if(options.GetTimeout().has_value()) {
			e.Timeout = options.GetTimeout().value();
		} else {
			e.Timeout = this->m_timeout;
		}

		if(options.GetSize().has_value()) {
			e.Size = options.GetSize().value();
		}

		if(!this->ValidateAndUpdateCapacity(e)) {
			throw std::overflow_error("Unable to add entry to memory cache: entry to big.");
		}

		this->m_data.insert({ entry.first, std::move(e) });
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	void MemoryCache<K, V, Equal, Hash, A>::InternalAddOrUpdateLocked(const EntryType& entry, const MemoryCacheEntryOptions& options, bool updateExisting)
	{
		auto result = this->ValidateCacheKey(entry.first, !updateExisting);

		if(result) {
			this->InternalUpdateLocked(entry, *result, options);
		} else {
			this->InternalAddLocked(entry, options);
		}
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	stl::ReferenceWrapper<MemoryCacheEntry<V>> MemoryCache<K, V, Equal, Hash, A>::ValidateCacheKey(const KeyType& key, bool throwIfExists)
	{
		auto iter = this->m_data.find(key);

		if(iter == this->m_data.end()) {
			return {};
		}

		auto& value = iter.value();
		ClockType::duration age = ClockType::now() - value.Timestamp;

		if(throwIfExists && age.count() < value.Timeout.count()) {
			throw std::out_of_range("Key already exists in memory cache!");
		}

		return stl::ReferenceWrapper<InternalEntryType>(value);
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	void MemoryCache<K, V, Equal, Hash, A>::ValidateCacheEntryOptions(const MemoryCacheEntryOptions& options)
	{
		if(this->m_capacity.has_value() && (!options.GetSize().has_value() || options.GetSize().value() <= 0)) {
			throw std::logic_error("Invalid cache entry options!");
		}
	}

	template <typename K, typename V, typename Equal, typename Hash, typename A>
	bool MemoryCache<K, V, Equal, Hash, A>::ValidateAndUpdateCapacity(const InternalEntryType& entry)
	{
		if(!this->m_capacity.has_value()) {
			return true;
		}

		auto newSize = this->m_size.value() + entry.Size;

		if(newSize > this->m_capacity.value()) {
			return false;
		}

		this->m_size.emplace(std::move(newSize));
		return true;
	}
	
	template <typename K, typename V, typename Equal, typename Hash, typename A>
	bool MemoryCache<K, V, Equal, Hash, A>::ValidateAndUpdateCapacityForUpdate(const InternalEntryType& entry, std::size_t newEntrySize)
	{
		if(!this->m_capacity.has_value()) {
			return true;
		}

		auto size = this->m_size.value();

		size -= entry.Size;
		size += newEntrySize;

		if(size > this->m_capacity.value()) {
			return false;
		}

		this->m_size.emplace(std::move(size));
		return true;
	}
	
	template<typename K, typename V, typename Equal, typename Hash, typename A>
	inline bool MemoryCache<K, V, Equal, Hash, A>::RawCleanupLocked()
	{
		std::size_t idx = 0;
		auto now = ClockType::now();

		for(
			auto iter = std::begin(this->m_data);
			iter != std::end(this->m_data);
			++iter
		) {
			if(idx >= CleanupBatchSize) {
				return false;
			}

			auto& value = iter.value();
			ClockType::duration age = now - value.Timestamp;
			idx += 1;

			if(age < value.Timeout) {
				continue;
			}

			this->m_data.erase(iter);
		}

		return true;
	}
}
