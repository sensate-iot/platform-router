/*
 * Generic in-memory cache.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/detail/memorycache.h>

namespace sensateiot::data
{
	using MemoryCacheEntryOptions = detail::MemoryCacheEntryOptions;
	
	template <typename K,
		typename V,
		typename Equal = std::equal_to<K>,
		typename Hash = detail::HashAlgorithm<K>,
		typename A = std::allocator<std::pair<K, detail::MemoryCacheEntry<V>>>
	>
	using MemoryCache = detail::MemoryCache<K, V, Equal, Hash, A>;
}
