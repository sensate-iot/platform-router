/*
 * Red-black tree node definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <utility>

namespace sensateiot::detail
{
	template <typename K, typename V, bool S>
	struct RedBlackTreeNode_base {
		RedBlackTreeNode_base() = default;
		RedBlackTreeNode_base(RedBlackTreeNode_base&& other) noexcept :
			m_key(std::move(other.m_key)), m_value(std::move(other.m_value)) {}
		explicit RedBlackTreeNode_base(K key, V value) : m_key(std::move(key)), m_value(std::move(value)) {}

		K m_key;
		V m_value;
	};

	template <typename K, typename V>
	struct RedBlackTreeNode_base<K, V, true> {
		RedBlackTreeNode_base() = default;
		explicit RedBlackTreeNode_base(K key) : m_key(std::move(key)) {}
		RedBlackTreeNode_base(RedBlackTreeNode_base&& other) noexcept : m_key(std::move(other.m_key)) {}

		K m_key;
	};

	template<typename K, typename V, typename Clk, typename C, bool S>
	struct RedBlackTreeNode : public RedBlackTreeNode_base<K, V, S> {
		typedef C ComparatorType;
		typedef K KeyType;
		typedef V ValueType;
		typedef Clk ClockType;
		typedef std::size_t HashType;

		enum class ColorType {
			RED,
			BLACK
		};

		explicit RedBlackTreeNode();
		template <bool is_set = S, std::enable_if_t<!is_set, int> = 0>
		explicit RedBlackTreeNode(KeyType key, ValueType value, HashType hash);
		template <bool is_set = S, std::enable_if_t<is_set, int> = 0>
		explicit RedBlackTreeNode(KeyType key, HashType hash);
		RedBlackTreeNode(RedBlackTreeNode &&rbnode) noexcept;
		RedBlackTreeNode(const RedBlackTreeNode &rbnode) = delete;

		~RedBlackTreeNode() = default;

		RedBlackTreeNode &operator=(const RedBlackTreeNode &rbnode) = delete;
		RedBlackTreeNode &operator=(RedBlackTreeNode &&rbnode) noexcept;

		[[nodiscard]]
		bool IsLeftChild() const;

		[[nodiscard]]
		bool IsRightChild() const;

		[[nodiscard]]
		RedBlackTreeNode* RightMost();

		[[nodiscard]]
		RedBlackTreeNode* LeftMost();

		[[nodiscard]]
		RedBlackTreeNode* Sibling();

		[[nodiscard]]
		const RedBlackTreeNode* Successor() const noexcept;

		[[nodiscard]]
		const RedBlackTreeNode* Predecessor() const noexcept;

		[[nodiscard]]
		RedBlackTreeNode* Successor() noexcept;

		[[nodiscard]]
		RedBlackTreeNode* Predecessor() noexcept;

		[[nodiscard]]
		RedBlackTreeNode* FindReplacement();

		[[nodiscard]]
		bool HasRedChild() const;

		void Swap(RedBlackTreeNode* other);

		bool operator==(const KeyType &input) const
		{
			return this->m_key == input;
		}

		bool operator!=(const KeyType &input) const
		{
			return this->m_key != input;
		}

		bool operator>(const KeyType &input) const
		{
			return this->m_key > input;
		}

		bool operator<(const KeyType &input) const
		{
			return this->m_key < input;
		}

		bool operator>=(const KeyType &input) const
		{
			return this->m_key >= input;
		}

		bool operator<=(const KeyType &input) const
		{
			return this->m_key <= input;
		}

		bool operator==(const RedBlackTreeNode &source) const
		{
			if(this->m_hash == source.m_hash) {
				return true;
			}

			return this->m_key == source.m_key;
		}

		bool operator!=(const RedBlackTreeNode &source) const
		{
			return !(*this == source);
		}

		bool operator>(const RedBlackTreeNode &source) const
		{
			return this->m_key > source.m_key;
		}

		bool operator<(const RedBlackTreeNode &source) const
		{
			return this->m_key < source.m_key;
		}

		bool operator>=(const RedBlackTreeNode &source) const
		{
			return this->m_key >= source.m_key;
		}

		bool operator<=(const RedBlackTreeNode &source) const
		{
			return this->m_key <= source.m_key;
		}

		ColorType m_color;
		HashType m_hash;
		typename ClockType::time_point m_created;
		ComparatorType m_cmp;

		RedBlackTreeNode* m_parent;
		RedBlackTreeNode* m_left;
		RedBlackTreeNode* m_right;
		stl::list_head m_entry;
	};
}

#include "redblacktreenode.hpp"
