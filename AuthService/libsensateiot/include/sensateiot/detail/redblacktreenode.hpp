/*
 * Red-black tree node definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

namespace sensateiot::detail
{
	template<typename K, typename V, typename Clk, typename C, bool S>
	inline RedBlackTreeNode<K, V, Clk, C, S>::RedBlackTreeNode() : m_color(ColorType::RED),
	                                                  m_hash(),
	                                                  m_created(ClockType::now()),
	                                                  m_parent(nullptr),
	                                                  m_left(nullptr),
	                                                  m_right(nullptr)
	{
	}

	template<typename K, typename V, typename Clk, typename C, bool S>
	template <bool is_set, std::enable_if_t<!is_set, int>>
	inline RedBlackTreeNode<K, V, Clk, C, S>::RedBlackTreeNode(KeyType key, ValueType value, HashType hash) :
			RedBlackTreeNode_base<K,V,S>(std::move(key), std::move(value)),
			m_color(ColorType::RED),
			m_hash(hash),
			m_created(ClockType::now()),
			m_parent(nullptr),
			m_left(nullptr), m_right(nullptr)
	{
	}

	template<typename K, typename V, typename Clk, typename C, bool S>
	template <bool is_set, std::enable_if_t<is_set, int>>
	inline RedBlackTreeNode<K, V, Clk, C, S>::RedBlackTreeNode(KeyType key, HashType hash) :
			RedBlackTreeNode_base<K,V,S>(std::move(key)),
			m_color(ColorType::RED),
			m_hash(hash),
			m_created(ClockType::now()),
			m_parent(nullptr),
			m_left(nullptr), m_right(nullptr)
	{
	}

	template<typename K, typename V, typename Clk, typename C, bool S>
	inline RedBlackTreeNode<K, V, Clk, C, S>::RedBlackTreeNode(RedBlackTreeNode &&rbnode) noexcept :
			RedBlackTreeNode_base<K,V,S>(std::forward<RedBlackTreeNode_base>(rbnode)),
			m_color(rbnode.m_color), m_hash(rbnode.m_hash), m_created(std::move(rbnode.m_created)),
			m_cmp(std::move(rbnode.m_cmp)),
			m_parent(rbnode.m_parent), m_left(rbnode.m_left), m_right(rbnode.m_right), m_entry(rbnode.m_entry)
	{
		rbnode.m_left = nullptr;
		rbnode.m_right = nullptr;
		rbnode.m_parent = nullptr;
	}

	template<typename K, typename V, typename Clk, typename C, bool S>
	inline RedBlackTreeNode <K, V, Clk, C, S> &RedBlackTreeNode<K, V, Clk, C, S>::operator=(RedBlackTreeNode&& rbnode) noexcept
	{
		this->m_right = rbnode.m_right;
		this->m_left = rbnode.m_left;
		this->m_parent = rbnode.m_parent;

		this->m_key = std::move(rbnode.m_key);
		this->m_color = rbnode.m_color;
		this->m_created = std::move(rbnode.m_created);
		this->m_hash = rbnode.m_hash;
		this->m_cmp = std::move(rbnode.m_cmp);
		this->m_entry = std::move(rbnode.m_entry);

		if constexpr (!S) {
			this->m_value = std::move(rbnode.m_value);
		}

		rbnode.m_left = nullptr;
		rbnode.m_right = nullptr;
		rbnode.m_parent = nullptr;

		return *this;
	}

	template<typename K, typename V, typename Clk, typename C, bool S>
	inline bool RedBlackTreeNode<K, V, Clk, C, S>::IsLeftChild() const
	{
		return (this->m_parent == nullptr) ? false : this->m_parent->m_left == this;
	}

	template<typename K, typename V, typename Clk, typename C, bool S>
	inline bool RedBlackTreeNode<K, V, Clk, C, S>::IsRightChild() const
	{
		return (this->m_parent == nullptr) ? false : this->m_parent->m_right == this;
	}

	template<typename K, typename V, typename Clk, typename C, bool S>
	inline RedBlackTreeNode <K, V, Clk, C, S> *RedBlackTreeNode<K, V, Clk, C, S>::RightMost()
	{
		RedBlackTreeNode *node = this;

		while(node->m_right != nullptr) {
			node = node->m_right;
		}

		return node;
	}

	template<typename K, typename V, typename Clk, typename C, bool S>
	inline RedBlackTreeNode <K, V, Clk, C, S> *RedBlackTreeNode<K, V, Clk, C, S>::LeftMost()
	{
		RedBlackTreeNode *node = this;

		while(node->m_left != nullptr) {
			node = node->m_left;
		}

		return node;
	}

	template<typename K, typename V, typename Clk, typename C, bool S>
	inline const RedBlackTreeNode <K, V, Clk, C, S> *RedBlackTreeNode<K, V, Clk, C, S>::Successor() const noexcept
	{
		if(this->m_right != nullptr) {
			return this->m_right->LeftMost();
		}

		if(this->IsLeftChild()) {
			return this->m_parent;
		}

		auto *successor = this;

		do {
			successor = successor->m_parent;
		} while(successor != nullptr && successor->IsRightChild());

		if(successor != nullptr) {
			return successor->m_parent;
		}

		return nullptr;
	}

	template<typename K, typename V, typename Clk, typename C, bool S>
	inline const RedBlackTreeNode <K, V, Clk, C, S> *RedBlackTreeNode<K, V, Clk, C, S>::Predecessor() const noexcept
	{
		if(this->m_left != nullptr) {
			return this->m_left->RightMost();
		}

		if(this->IsRightChild()) {
			return this->m_parent;
		}

		auto *predeccesor = this;

		do {
			predeccesor = predeccesor->m_parent;
		} while(predeccesor != nullptr && predeccesor->IsLeftChild());

		if(predeccesor != nullptr) {
			return predeccesor->m_parent;
		}

		return nullptr;
	}

	template<typename K, typename V, typename Clk, typename C, bool S>
	inline RedBlackTreeNode<K, V, Clk, C, S> *RedBlackTreeNode<K, V, Clk, C, S>::Predecessor() noexcept
	{
		auto* predecessor = std::as_const(*this).Predecessor();
		return const_cast<RedBlackTreeNode*>(predecessor);
	}

	template<typename K, typename V, typename Clk, typename C, bool S>
	inline RedBlackTreeNode<K, V, Clk, C, S> *RedBlackTreeNode<K, V, Clk, C, S>::Successor() noexcept
	{
		auto* successor = std::as_const(*this).Successor();
		return const_cast<RedBlackTreeNode*>(successor);
	}

	template<typename K, typename V, typename Clk, typename C, bool S>
	inline RedBlackTreeNode <K, V, Clk, C, S> *RedBlackTreeNode<K, V, Clk, C, S>::Sibling()
	{
		if(this->m_parent == nullptr) {
			return nullptr;
		}

		if(this->IsLeftChild()) {
			return this->m_parent->m_right;
		}

		return this->m_parent->m_left;
	}

	template<typename K, typename V, typename Clk, typename C, bool S>
	inline RedBlackTreeNode<K,V,Clk, C, S> *RedBlackTreeNode<K, V, Clk, C, S>::FindReplacement()
	{
		if(this->m_left != nullptr && this->m_right != nullptr) {
			return this->m_right->LeftMost(); /* Successor */
		}

		if(this->m_right == nullptr && this->m_left == nullptr) {
			return nullptr;
		}

		if(this->m_left != nullptr) {
			return this->m_left;
		}

		return this->m_right;
	}

	template<typename K, typename V, typename Clk, typename C, bool S>
	inline void RedBlackTreeNode<K, V, Clk, C, S>::Swap(RedBlackTreeNode* other)
	{
		std::swap(this->m_key, other->m_key);
		std::swap(this->m_hash, other->m_hash);
		std::swap(this->m_created, other->m_created);
		std::swap(this->m_cmp, other->m_cmp);

		if constexpr (!S) {
			std::swap(this->m_value, other->m_value);
		}
	}

	template<typename K, typename V, typename Clk, typename C, bool S>
	inline bool RedBlackTreeNode<K, V, Clk, C, S>::HasRedChild() const
	{
		return (this->m_left != nullptr && this->m_left->m_color == ColorType::RED) ||
				(this->m_right != nullptr && this->m_right->m_color == ColorType::RED);
	}
}
