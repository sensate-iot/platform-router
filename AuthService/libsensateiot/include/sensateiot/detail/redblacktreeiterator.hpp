/*
 * Iterator type for red-black tree's.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

namespace sensateiot::detail
{
	template<typename K, typename V, typename C, typename H, bool is_const, bool is_key_iter>
	RedBlackTreeIterator<K, V, C, H, is_const, is_key_iter>::RedBlackTreeIterator(NodeType &node) : m_node(node)
	{
	}

	template<typename K, typename V, typename C, typename H, bool is_const, bool is_key_iter>
	RedBlackTreeIterator<K, V, C, H, is_const, is_key_iter>::RedBlackTreeIterator(const RedBlackTreeIterator &other) : m_node(other.m_node)
	{
	}

	template<typename K, typename V, typename C, typename H, bool is_const, bool is_key_iter>
	RedBlackTreeIterator <K, V, C, H, is_const, is_key_iter> &
	RedBlackTreeIterator<K, V, C, H, is_const, is_key_iter>::operator=(const RedBlackTreeIterator &other)
	{
		if(this == &other) {
			return *this;
		}

		this->m_node = other.m_node;
		return *this;
	}

	template<typename K, typename V, typename C, typename H, bool is_const, bool is_key_iter>
	RedBlackTreeIterator <K, V, C, H, is_const, is_key_iter>
	RedBlackTreeIterator<K, V, C, H, is_const, is_key_iter>::operator++(int)
	{
		RedBlackTreeIterator iter = *this;

		++(*this);
		return iter;
	}

	template<typename K, typename V, typename C, typename H, bool is_const, bool is_key_iter>
	RedBlackTreeIterator <K, V, C, H, is_const, is_key_iter> &
	RedBlackTreeIterator<K, V, C, H, is_const, is_key_iter>::operator++()
	{
		if(this->m_node) {
			this->m_node = stl::ReferenceWrapper<NodeType>(*this->m_node->Successor());
		}

		return *this;
	}

	template<typename K, typename V, typename C, typename H, bool is_const, bool is_key_iter>
	RedBlackTreeIterator <K, V, C, H, is_const, is_key_iter> &
	RedBlackTreeIterator<K, V, C, H, is_const, is_key_iter>::operator--()
	{
		if(this->m_node) {
			this->m_node = stl::ReferenceWrapper<NodeType>(*this->m_node->Predecessor());
		}

		return *this;
	}

	template <typename K, typename V, typename C, typename H, bool is_const, bool is_key_iter>
	RedBlackTreeIterator<K, V, C, H, is_const, is_key_iter>
	RedBlackTreeIterator<K, V, C, H, is_const, is_key_iter>:: operator--(int)
	{
		auto rv = *this;

		--(*this);
		return rv;
	}

	template<typename K, typename V, typename C, typename H, bool is_const, bool is_key_iter>
	boost::chrono::high_resolution_clock::time_point
	RedBlackTreeIterator<K, V, C, H, is_const, is_key_iter>::CreatedAt() const
	{
		return this->m_node->CreatedAt;
	}
}
