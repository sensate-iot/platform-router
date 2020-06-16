/*
 * STL map definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot.h>
#include <sensateiot/detail/redblacktree.h>

namespace sensateiot::stl
{
	template <typename K, typename V, typename C = detail::Compare<K>, typename H = detail::HashAlgorithm<K>>
	class DLL_EXPORT Map : public detail::RedBlackTree<K, V, C, H> {
		typedef detail::RedBlackTree<K, V, C, H> Base;
		static constexpr typename Base::SizeType DefaultTimeout = Base::DefaultTimeout;

	public:
		explicit Map(long timeout = DefaultTimeout) : detail::RedBlackTree<K,V,C,H>(timeout) {}
	};

	template <typename K, typename C = detail::Compare<K>, typename H = detail::HashAlgorithm<K>>
	class DLL_EXPORT Set : public detail::RedBlackTree<K, void, C, H> {
		typedef detail::RedBlackTree<K, void, C, H> Base;
		static constexpr typename Base::SizeType DefaultTimeout = Base::DefaultTimeout;

	public:
		explicit Set(long timeout = DefaultTimeout) : detail::RedBlackTree<K,void,C,H>(timeout) {}
	};
}
