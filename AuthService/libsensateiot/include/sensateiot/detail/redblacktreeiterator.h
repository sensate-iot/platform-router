/*
 * Iterator type for red-black tree's.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <sensateiot/stl/referencewrapper.h>

#include <shared_mutex>
#include <type_traits>

namespace sensateiot::detail
{
	template<typename K, typename V, typename C, typename H>
	class RedBlackTree;

	template<typename K, typename V, typename C, typename H>
	class RedBlackTree_base;

	template<typename K, typename V, typename C, typename H, bool is_const, bool is_key_iter>
	class RedBlackTreeIterator {
		typedef typename std::conditional<is_const,
				const typename RedBlackTree<K, V, C, H>::NodeType,
				typename RedBlackTree<K, V, C, H>::NodeType>::type NodeType;

		typedef boost::chrono::high_resolution_clock ClockType;

	public:
		static constexpr bool IsConst = is_const;
		static constexpr bool IsKeyIterator = is_key_iter;
		static constexpr bool IsValueIterator = !is_key_iter;

		typedef typename std::conditional<IsConst,
				const RedBlackTree<K,V,C,H>,
				 RedBlackTree<K,V,C,H>>::type TreeType;

		typedef typename std::conditional<IsConst,
				const typename TreeType::ValueType,
				typename TreeType::ValueType>::type ValueType;

		typedef typename std::conditional<IsConst,
				const typename TreeType::KeyType,
				typename TreeType::KeyType>::type KeyType;

		typedef typename std::conditional<IsConst,
				const typename TreeType::NodeType *,
				typename TreeType::NodeType *>::type PointerType;

		typedef typename std::conditional<IsConst,
				const typename TreeType::NodeType &,
				typename TreeType::NodeType &>::type ReferenceType;

		RedBlackTreeIterator() = default;
		explicit RedBlackTreeIterator(NodeType& node);
		RedBlackTreeIterator(const RedBlackTreeIterator& other);
		RedBlackTreeIterator(RedBlackTreeIterator&& other) noexcept = default;
		~RedBlackTreeIterator() = default;

		RedBlackTreeIterator& operator=(const RedBlackTreeIterator& other);
		RedBlackTreeIterator& operator=(RedBlackTreeIterator&& other) noexcept = default;

		RedBlackTreeIterator operator++(int);
		RedBlackTreeIterator &operator++();

		RedBlackTreeIterator &operator--();
		RedBlackTreeIterator operator--(int);

		[[nodiscard]]
		ClockType::time_point CreatedAt() const;

		bool operator==(const RedBlackTreeIterator& other) const
		{
			if(!this->m_node && !other.m_node) {
				return true;
			}

			if(!this->m_node || !other.m_node) {
				return false;
			}

			return *this->m_node == *other.m_node ;
		}

		bool operator!=(const RedBlackTreeIterator& other) const
		{
			return !(*this == other);
		}

		template<bool k = is_key_iter, typename = typename std::enable_if<k>::type>
		const KeyType &operator*() const
		{
			return this->m_node.get().m_key;
		}

		template<bool k = is_key_iter, typename = typename std::enable_if<!k>::type>
		const ValueType &operator*() const
		{
			return this->m_node.get().m_value;
		}

		template<bool k = is_key_iter, typename = typename  std::enable_if<k>::type>
		const KeyType *operator->() const
		{
			return &this->m_node.get().m_key;
		}

		template<bool k = is_key_iter, typename = typename std::enable_if<!k>::type>
		const ValueType *operator->() const
		{
			return &this->m_node.get().m_value;
		}

		template<bool k = is_key_iter, typename = typename std::enable_if<k>::type>
		KeyType &operator*()
		{
			return this->m_node.get().m_key;
		}

		template<bool k = is_key_iter, typename = typename std::enable_if<!k>::type>
		ValueType &operator*()
		{
			return this->m_node.get().m_value;
		}

		template<bool k = is_key_iter, typename = typename std::enable_if<k>::type>
		KeyType *operator->()
		{
			return &this->m_node.get().m_key;
		}

		template<bool k = is_key_iter, typename = typename std::enable_if<!k>::type>
		ValueType *operator->()
		{
			return &this->m_node.get().m_value;
		}

	private:
		stl::ReferenceWrapper<NodeType> m_node;

		friend class RedBlackTree_base<K,V,C,H>;
		friend class RedBlackTree<K,V,C,H>;
	};
}

#include "redblacktreeiterator.hpp"
