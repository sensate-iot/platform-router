/*
 * Red-black tree definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <vector>
#include <stdexcept>

namespace sensateiot::detail
{
	namespace algo
	{
		template<typename T>
		inline typename MurmurHash3<T>::HashType MurmurHash3<T>::operator()(const T *array, std::size_t size) const
		{
			HashType h1 = seed;
			HashType k1 = 0;

			const uint32_t c1 = 0xcc9e2d51;
			const uint32_t c2 = 0x1b873593;

			const size_t bsize = sizeof(k1);
			const size_t nblocks = size / bsize;
			const auto *bytes = static_cast<const std::uint8_t *>(array);

			if constexpr (!std::is_same<T, std::uint8_t>::value) {
				size *= sizeof(T);
			}

			/* body */
			for(size_t i = 0; i < nblocks; i++, bytes += bsize) {
				memcpy(&k1, bytes, bsize);

				k1 *= c1;
				k1 = rotl32(k1, 15U);
				k1 *= c2;

				h1 ^= k1;
				h1 = rotl32(h1, 13U);
				h1 = h1 * 5 + 0xe6546b64;
			}

			k1 = 0;

			/* tail */
			switch(size & 3) {
			case 3:
				k1 ^= (static_cast<uint32_t>(bytes[2])) << 16;

			case 2:
				k1 ^= (static_cast<uint32_t>(bytes[1])) << 8;

			case 1:
				k1 ^= bytes[0];
				k1 *= c1;
				k1 = rotl32(k1, 15U);
				k1 *= c2;
				h1 ^= k1;

			default:
				break;
			}

			/* finalization */
			h1 ^= static_cast<uint32_t>(size);
			h1 = fmix32(h1);

			return h1;
		}

		template<typename T>
		inline typename FnvHash<T>::HashType FnvHash<T>::operator()(const T *array, std::size_t size) const
		{
			const auto *bytes = static_cast<const std::uint8_t *>(array);
			auto hash = Seed;

			if constexpr (!std::is_same<T, std::uint8_t>::value) {
				size *= sizeof(T);
			}

			for(std::size_t idx{0}; idx < size; idx++) {
				hash ^= bytes[idx];
				hash *= Prime;
			}

			return hash;
		}
	}

	template<typename K, typename V, typename C, typename H>
	inline RedBlackTree_base<K, V, C, H>::RedBlackTree_base(long timeout) :
			m_root(nullptr), m_size(0UL), m_timeout(timeout), m_algo()
	{
	}

	template<typename K, typename V, typename C, typename H>
	inline RedBlackTree_base<K, V, C, H>::RedBlackTree_base(boost::chrono::milliseconds tmo) :
			m_root(nullptr), m_size(0UL), m_timeout(tmo.count()), m_algo()
	{
	}

	template<typename K, typename V, typename C, typename H>
	inline RedBlackTree_base<K, V, C, H>::RedBlackTree_base(RedBlackTree_base &&other) noexcept :
			m_root(nullptr), m_size(0UL), m_timeout(0L), m_algo(std::move(other.m_algo))
	{
		std::unique_lock lock(other.m_lock);

		this->m_allocator = std::move(other.m_allocator);
		this->m_compare = std::move(other.m_compare);
		this->m_root = other.m_root;
		this->m_size = other.m_size;
		this->m_timeout = other.m_timeout;

		other.m_root = nullptr;
		other.m_size = 0UL;
	}

	template<typename K, typename V, typename C, typename H>
	inline RedBlackTree_base<K, V, C, H>::RedBlackTree_base(const RedBlackTree_base &other) : m_root(nullptr), m_size(0UL),
	                                                                                   m_timeout(0L), m_algo(other.m_algo)
	{
		std::shared_lock lock(other.m_lock);

		this->m_allocator = other.m_allocator;
		this->m_timeout = other.m_timeout;
		this->m_compare = other.m_compare;
		this->Copy(other.m_root);
	}

	template<typename K, typename V, typename C, typename H>
	inline RedBlackTree_base<K, V, C, H>::~RedBlackTree_base()
	{
		this->Clear();
	}

	template<typename K, typename V, typename C, typename H>
	inline RedBlackTree_base <K, V, C, H> &RedBlackTree_base<K, V, C, H>::operator=(RedBlackTree_base &&rhs) noexcept
	{
		if(this == &rhs) {
			return *this;
		}

		this->Move(rhs);
		return *this;
	}

	template<typename K, typename V, typename C, typename H>
	inline RedBlackTree_base <K, V, C, H> &RedBlackTree_base<K, V, C, H>::operator=(const RedBlackTree_base &rhs)
	{
		if(this == &rhs) {
			return *this;
		}

		this->Copy(rhs);
		return *this;
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::Clear()
	{
		std::unique_lock lock(this->m_lock);

		if(this->m_root == nullptr) {
			return;
		}

		this->Chop(this->m_root);
		this->m_root = nullptr;
		this->m_size = 0UL;
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::Chop(NodeType *node) noexcept
	{
		if(node == nullptr) {
			return;
		}

		this->Chop(node->m_left);
		this->Chop(node->m_right);

		stl::list_del(&node->m_entry);
		std::allocator_traits<AllocatorType>::destroy(this->m_allocator, node);
		this->m_allocator.deallocate(node, 1);
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::Move(RedBlackTree_base &other) noexcept
	{
		if(this == &other) {
			return;
		}

		std::unique_lock myLock(this->m_lock, std::defer_lock);
		std::unique_lock otherLock(other.m_lock, std::defer_lock);
		std::lock(myLock, otherLock);

		this->Chop(this->m_root);
		this->m_allocator = std::move(other.m_allocator);
		this->m_root = other.m_root;
		this->m_size = other.m_size;
		this->m_timeout = other.m_timeout;
		this->m_compare = std::move(other.m_compare);
		this->m_algo = std::move(other.m_algo);

		other.m_root = nullptr;
		other.m_size = 0UL;
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::Copy(const RedBlackTree_base &other)
	{
		if(this == &other) {
			return;
		}

		std::unique_lock myLock(this->m_lock, std::defer_lock);
		std::shared_lock otherLock(other.m_lock, std::defer_lock);
		std::lock(myLock, otherLock);

		this->m_allocator = other.m_allocator;
		this->m_timeout = other.m_timeout;
		this->m_compare = other.m_compare;
		this->m_algo = other.m_algo;

		this->Chop(this->m_root);
		this->m_size = 0UL;
		this->m_root = nullptr;
		this->Copy(other.m_root);
	}

	template<typename K, typename V, typename C, typename H>
	inline bool RedBlackTree_base<K, V, C, H>::Empty() const
	{
		std::shared_lock l(this->m_lock);
		return this->m_size == 0UL;
	}

	template<typename K, typename V, typename C, typename H>
	inline typename RedBlackTree_base<K, V, C, H>::SizeType RedBlackTree_base<K, V, C, H>::Size() const
	{
		std::shared_lock l(this->m_lock);
		return this->m_size;
	}


	template<typename K, typename V, typename C, typename H>
	inline bool RedBlackTree_base<K, V, C, H>::operator==(const RedBlackTree_base &other) const
	{
		std::shared_lock myLock(this->m_lock, std::defer_lock);
		std::shared_lock otherLock(other.m_lock, std::defer_lock);
		std::lock(myLock, otherLock);

		if(this->m_size != other.m_size) {
			return false;
		}

		auto iter = this->Begin();
		auto otherIter = other.Begin();

		while(iter != this->End() && otherIter != other.End()) {
			if(iter != otherIter) {
				return false;
			}

			++iter;
			++otherIter;
		}

		return true;
	}

	template<typename K, typename V, typename C, typename H>
	inline bool RedBlackTree_base<K, V, C, H>::operator!=(const RedBlackTree_base &other) const
	{
		return !(*this == other);
	}

	template<typename K, typename V, typename C, typename H>
	inline bool RedBlackTree_base<K, V, C, H>::Contains(const KeyType &key) const
	{
		return this->Find(key) != this->End();
	}

	template<typename K, typename V, typename C, typename H>
	inline typename RedBlackTree_base<K, V, C, H>::Iterator
	RedBlackTree_base<K, V, C, H>::Find(const KeyType &key, TickType now)
	{
		auto rv = this->RawFind(key, now);

		if(rv.second == std::as_const(*this).End()) {
			return Iterator();
		}

		Iterator iter(const_cast<NodeType &>(rv.second.m_node.get()));

		if(!rv.first) {
			this->Erase(iter);
			return Iterator();
		}

		return iter;
	}

	template<typename K, typename V, typename C, typename H>
	typename RedBlackTree_base<K, V, C, H>::ConstIterator
	inline RedBlackTree_base<K, V, C, H>::Find(const KeyType &key, TickType now) const
	{
		auto rv = this->RawFind(key, now);

		if(!rv.first) {
			return ConstIterator();
		}

		return rv.second;
	}

	template<typename K, typename V, typename C, typename H>
	inline std::pair<bool, typename RedBlackTree_base<K, V, C, H>::ConstIterator>
	RedBlackTree_base<K, V, C, H>::RawFind(const KeyType &key, TickType now) const noexcept
	{
		std::shared_lock l(this->m_lock);

		if(this->m_root == nullptr) {
			return std::make_pair(false, this->End());
		}

		const auto *current = this->m_root;
		auto found = this->End();
		const auto hash = this->m_algo(key);

		do {
			if(hash < current->m_hash) {
				current = current->m_left;
			} else if(hash > current->m_hash) {
				current = current->m_right;
			} else {
				auto cmp = this->m_compare(key, current->m_key);

				if(cmp == 0) {
					found = ConstIterator(*current);
					break;
				}

				if(cmp < 0) {
					current = current->m_left;
				} else if(cmp > 0) {
					current = current->m_right;
				}
			}
		} while(current != nullptr);

		if(found == this->End() || current == nullptr) {
			return std::make_pair(false, std::move(found));
		}

		using Millis = boost::chrono::milliseconds;
		const ClockType::duration age = now - current->m_created;

		return std::make_pair(boost::chrono::duration_cast<Millis>(age).count() <= this->m_timeout, std::move(found));
	}

	template<typename K, typename V, typename C, typename H>
	template<bool is_set, std::enable_if_t<is_set, int>>
	inline bool RedBlackTree_base<K, V, C, H>::operator[](const KeyType &k) const
	{
		auto result = this->Find(k);
		return result != this->End();
	}

	template<typename K, typename V, typename C, typename H>
	template<bool is_set, std::enable_if_t<!is_set, int>>
	inline typename RedBlackTree_base<K, V, C, H>::ValueType &RedBlackTree_base<K, V, C, H>::operator[](const KeyType &k)
	{
		static_assert(std::is_default_constructible_v<V>, "V should be default constructable!");

		auto result = this->Find(k);

		if(result == this->End()) {
			ValueType v;
			auto rv = this->Insert(k, v);

			if(!rv.second) {
				throw std::overflow_error("Unable to create element");
			}

			return *rv.first;
		}

		return *result;
	}

	template<typename K, typename V, typename C, typename H>
	template<bool is_set, std::enable_if_t<!is_set, int>>
	inline const typename RedBlackTree_base<K, V, C, H>::ValueType &
	RedBlackTree_base<K, V, C, H>::operator[](const KeyType &k) const
	{
		auto iter = this->Find(k);

		if(iter == this->End()) {
			throw std::out_of_range(KeyNotFound.data());
		}

		return *iter;
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::RotateLeft(NodeType *node)
	{
		if(node->m_right == nullptr) {
			return;
		}

		auto *y = node->m_right;
		auto *b = y->m_left;
		auto *f = node->m_parent;

		if(f == nullptr) {
			y->m_parent = nullptr;
			this->m_root = y;
		} else {
			y->m_parent = f;

			if(f->m_left == node) {
				f->m_left = y;
			}

			if(f->m_right == node) {
				f->m_right = y;
			}
		}

		y->m_left = node;
		node->m_parent = y;
		node->m_right = b;

		if(b != nullptr) {
			b->m_parent = node;
		}
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::RotateRight(NodeType *node)
	{
		if(node->m_left == nullptr) {
			return;
		}

		auto *x = node->m_left;
		auto *b = x->m_right;
		auto *f = node->m_parent;

		if(f == nullptr) {
			x->m_parent = nullptr;
			this->m_root = x;
		} else {
			x->m_parent = f;

			if(f->m_left == node) {
				f->m_left = x;
			}

			if(f->m_right == node) {
				f->m_right = x;
			}
		}

		x->m_right = node;
		node->m_parent = x;
		node->m_left = b;

		if(b != nullptr) {
			b->m_parent = node;
		}
	}

	template<typename K, typename V, typename C, typename H>
	template<bool is_set, std::enable_if_t<!is_set, int>>
	inline std::pair<typename RedBlackTree_base<K, V, C, H>::Iterator, bool>
	RedBlackTree_base<K, V, C, H>::Insert(Iterator iter, const ValueType &value)
	{
		if(iter == this->End()) {
			return std::make_pair(std::move(iter), false);
		}

		iter.m_node->m_value = value;
		return std::make_pair(std::move(iter), true);
	}

	template<typename K, typename V, typename C, typename H>
	template<bool is_set, std::enable_if_t<!is_set, int>>
	inline std::pair<typename RedBlackTree_base<K, V, C, H>::Iterator, bool>
	RedBlackTree_base<K, V, C, H>::Insert(Iterator iter, ValueType &&value)
	{
		if(iter == this->End()) {
			return std::make_pair(std::move(iter), false);
		}

		iter.m_node->m_value = std::forward<ValueType>(value);
		return std::make_pair(std::move(iter), true);
	}

	template<typename K, typename V, typename C, typename H>
	inline std::pair<typename RedBlackTree_base<K, V, C, H>::Iterator, bool>
	RedBlackTree_base<K, V, C, H>::Insert(NodeType *node)
	{
		NodeType *q;
		int cmp = 0;

		auto p = this->m_root;

		if(this->m_root == nullptr) {
			this->m_root = node;
			node->m_parent = nullptr;
			node->m_color = NodeType::ColorType::BLACK;
		} else {
			do {
				q = p;

				if(node->m_hash < p->m_hash) {
					p = p->m_left;
				} else if(node->m_hash > p->m_hash) {
					p = p->m_right;
				} else {
					cmp = this->m_compare(node->m_key, p->m_key);

					if(cmp < 0) {
						p = p->m_left;
					} else if(cmp > 0) {
						p = p->m_right;
					} else {
						/*
						 * Key is already in the tree:
						 *
						 * 1. Move value from created node into the node already in the tree;
						 * 2. Delete the created node.
						 */

						if constexpr (!IsSet) {
							p->m_value = std::move(node->m_value);
						}

						p->m_created = std::move(node->m_created);

						stl::list_del(&p->m_entry);
						stl::list_head_init(&p->m_entry);
						stl::list_add_tail(&p->m_entry, &this->m_tmo_queue);

						std::allocator_traits<AllocatorType>::destroy(this->m_allocator, node);
						this->m_allocator.deallocate(node, 1);


						return std::make_pair(Iterator(*p), true);
					}
				}
			} while(p != nullptr);

			node->m_parent = q;

			if(q->m_hash > node->m_hash) {
				q->m_left = node;
			} else if(q->m_hash < node->m_hash) {
				q->m_right = node;
			} else {
				if(cmp < 0) {
					q->m_left = node;
				} else {
					q->m_right = node;
				}
			}
		}

		++this->m_size;
		this->FixInsert(node);
		stl::list_add_tail(&node->m_entry, &this->m_tmo_queue);

		return std::make_pair(Iterator(*node), true);
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::FixInsert(NodeType *node)
	{
		auto *x = node;

		while(x != this->m_root && x->m_parent->m_color == NodeType::ColorType::RED) {
			if(x->m_parent == x->m_parent->m_parent->m_left) {
				auto *y = x->m_parent->m_parent->m_right;

				if((y != nullptr) && (y->m_color == NodeType::ColorType::RED)) {
					x->m_parent->m_color = NodeType::ColorType::BLACK;
					y->m_color = NodeType::ColorType::BLACK;
					x->m_parent->m_parent->m_color = NodeType::ColorType::RED;
					x = x->m_parent->m_parent;
				} else {
					if(x->m_parent->m_right == x) {
						x = x->m_parent;
						this->RotateLeft(x);
					}

					x->m_parent->m_color = NodeType::ColorType::BLACK;
					x->m_parent->m_parent->m_color = NodeType::ColorType::RED;
					this->RotateRight(x->m_parent->m_parent);
				}
			} else {
				auto *y = x->m_parent->m_parent->m_left;

				if((y != nullptr) && (y->m_color == NodeType::ColorType::RED)) {
					x->m_parent->m_color = NodeType::ColorType::BLACK;
					y->m_color = NodeType::ColorType::BLACK;
					x->m_parent->m_parent->m_color = NodeType::ColorType::RED;
					x = x->m_parent->m_parent;
				} else {
					if(x->m_parent->m_left == x) {
						x = x->m_parent;
						this->RotateRight(x);
					}

					x->m_parent->m_color = NodeType::ColorType::BLACK;
					x->m_parent->m_parent->m_color = NodeType::ColorType::RED;
					this->RotateLeft(x->m_parent->m_parent);
				}
			}
		}

		this->m_root->m_color = NodeType::ColorType::BLACK;
	}

	template<typename K, typename V, typename C, typename H>
	template<bool is_set, std::enable_if_t<is_set, int>>
	inline std::pair<typename RedBlackTree_base<K, V, C, H>::Iterator, bool>
	RedBlackTree_base<K, V, C, H>::Insert(const KeyType &key)
	{
		auto hash = this->m_algo(key);
		auto *node = this->m_allocator.allocate(1);
		std::allocator_traits<AllocatorType>::construct(this->m_allocator, node, key, hash);

		std::unique_lock l(this->m_lock);
		return this->Insert(node);
	}

	template<typename K, typename V, typename C, typename H>
	template<bool is_set, std::enable_if_t<!is_set, int>>
	inline std::pair<typename RedBlackTree_base<K, V, C, H>::Iterator, bool>
	RedBlackTree_base<K, V, C, H>::Insert(const KeyType &key, const ValueType &value)
	{
		const auto hash = this->m_algo(key);
		auto *node = this->m_allocator.allocate(1);
		std::allocator_traits<AllocatorType>::construct(this->m_allocator, node, key, value, hash);

		std::unique_lock lck(this->m_lock);
		return this->Insert(node);
	}

	template<typename K, typename V, typename C, typename H>
	inline std::pair<bool, typename RedBlackTree_base<K, V, C, H>::SizeType>
	RedBlackTree_base<K, V, C, H>::Validate(const NodeType *node) const
	{
		bool success = true;

		if(node == nullptr) {
			return std::make_pair(true, 0ULL);
		}

		SizeType add;
		auto &log = util::Log::GetLog();
		constexpr auto endl = util::Log::NewLine;

		if(node->m_color == NodeType::ColorType::RED) {
			if(node->m_left && node->m_left->m_color != NodeType::ColorType::BLACK) {
				log << "Bad color detected!" << endl;
				success = false;
			}

			if(node->m_right && node->m_right->m_color != NodeType::ColorType::BLACK) {
				log << "Bad color detected!" << endl;
				success = false;
			}

			add = 0ULL;
		} else {
			add = 1ULL;
		}

		if(node->m_left && node->m_left->m_hash > node->m_hash) {
			log << "Invalid sort!" << endl;
			log << "Node hash: " << node->m_hash << " Left side: " << node->m_left->m_hash << endl;
			success = false;
		}

		if(node->m_right && node->m_right->m_hash < node->m_hash) {
			log << "Invalid sort!" << endl;
			log << "Node hash: " << node->m_hash << " Right side: " << node->m_right->m_hash << endl;
			success = false;
		}

		auto left = this->Validate(node->m_left);
		auto right = this->Validate(node->m_right);

		if(left.second != right.second) {
			log << "Invalid black count!" << endl;
			success = false;
		}

		success = success && left.first && right.first;
		return std::make_pair(success, left.second + add);
	}

	template<typename K, typename V, typename C, typename H>
	inline bool RedBlackTree_base<K, V, C, H>::Validate() const
	{
		auto result = this->Validate(this->m_root);
		return result.first;
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::DeleteFix(NodeType *node)
	{
		if(this->m_root == node) {
			return;
		}

		auto *sibling = node->Sibling();
		auto *parent = node->m_parent;

		if(sibling == nullptr) {
			this->DeleteFix(parent);
			return;
		}

		if(sibling->m_color == NodeType::ColorType::RED) {
			parent->m_color = NodeType::ColorType::RED;
			sibling->m_color = NodeType::ColorType::BLACK;
			if(sibling->IsLeftChild()) {
				this->RotateRight(parent);
			} else {
				this->RotateLeft(parent);
			}

			this->DeleteFix(node);
		} else {
			if(sibling->HasRedChild()) {
				if(sibling->m_left != nullptr && sibling->m_left->m_color == NodeType::ColorType::RED) {
					if(sibling->IsLeftChild()) {
						sibling->m_left->m_color = sibling->m_color;
						sibling->m_color = parent->m_color;
						this->RotateRight(parent);
					} else {
						sibling->m_left->m_color = parent->m_color;
						this->RotateRight(sibling);
						this->RotateLeft(parent);
					}
				} else {
					if(sibling->IsLeftChild()) {
						sibling->m_right->m_color = parent->m_color;
						this->RotateLeft(sibling);
						this->RotateRight(parent);
					} else {
						sibling->m_right->m_color = sibling->m_color;
						sibling->m_color = parent->m_color;
						this->RotateLeft(parent);
					}
				}

				parent->m_color = NodeType::ColorType::BLACK;
			} else {
				sibling->m_color = NodeType::ColorType::RED;
				if(parent->m_color == NodeType::ColorType::BLACK) {
					this->DeleteFix(parent);
				} else {
					parent->m_color = NodeType::ColorType::BLACK;
				}
			}
		}
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::Erase(Iterator first, Iterator last)
	{
		std::vector<NodeType *> removes;
		std::unique_lock l(this->m_lock);

		for(auto iter = first; iter != last; ++iter) {
			removes.push_back(iter.m_node.get_ptr());
		}


		for(auto entry : removes) {
			this->Erase(entry);
			--this->m_size;
		}
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::Erase(const KeyType &key)
	{
		std::unique_lock l(this->m_lock);

		auto *current = this->m_root;
		const auto hash = this->m_algo(key);

		while(current != nullptr) {
			if(hash < current->m_hash) {
				current = current->m_left;
			} else if(hash > current->m_hash) {
				current = current->m_right;
			} else {
				auto cmp = this->m_compare(key, current->m_key);

				if(cmp < 0) {
					current = current->m_left;
					continue;
				} else if(cmp > 0) {
					current = current->m_right;
					continue;
				}

				break;
			}
		}

		if(current == nullptr) {
			return;
		}

		this->Erase(current);
		--this->m_size;
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::Erase(Iterator pos)
	{
		std::unique_lock l(this->m_lock);

		this->Erase(pos.m_node.get_ptr());
		--this->m_size;
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::Erase(NodeType *v)
	{
		auto *u = v->FindReplacement();
		auto uvBlack = ((u == nullptr || u->m_color == NodeType::ColorType::BLACK) &&
		                v->m_color == NodeType::ColorType::BLACK
		);
		auto *parent = v->m_parent;

		if(u == nullptr) {
			if(v == this->m_root) {
				this->m_root = nullptr;
			} else {
				if(uvBlack) {
					this->DeleteFix(v);
				} else if(v->Sibling() != nullptr) {
					v->Sibling()->m_color = NodeType::ColorType::RED;
				}

				if(v->IsLeftChild()) {
					parent->m_left = nullptr;
				} else {
					parent->m_right = nullptr;
				}
			}

			stl::list_del(&v->m_entry);
			v->m_parent = v->m_right = v->m_left = nullptr;
			std::allocator_traits<AllocatorType>::destroy(this->m_allocator, v);
			this->m_allocator.deallocate(v, 1);

			return;
		}

		if(v->m_left == nullptr || v->m_right == nullptr) {
			if(v == this->m_root) {
				this->Swap(v, u);

				u->m_parent = u->m_right = u->m_left = nullptr;
				stl::list_del(&v->m_entry);
				std::allocator_traits<AllocatorType>::destroy(this->m_allocator, v);
				this->m_allocator.deallocate(v, 1);
			} else {
				if(v->IsLeftChild()) {
					parent->m_left = u;
				} else {
					parent->m_right = u;
				}

				stl::list_del(&v->m_entry);
				v->m_parent = v->m_left = v->m_right = nullptr;
				std::allocator_traits<AllocatorType>::destroy(this->m_allocator, v);
				this->m_allocator.deallocate(v, 1);

				u->m_parent = parent;
				if(uvBlack) {
					this->DeleteFix(u);
				} else {
					u->m_color = NodeType::ColorType::BLACK;
				}
			}

			return;
		}

		this->Swap(v, u);
		this->Erase(v);
	}

	template<typename K, typename V, typename C, typename H>
	template<typename... Args>
	inline std::pair<typename RedBlackTree_base<K, V, C, H>::Iterator, bool>
	RedBlackTree_base<K, V, C, H>::Emplace(Args &&... args)
	{
		return this->Insert(std::forward<Args>(args)...);
	}

	template<typename K, typename V, typename C, typename H>
	template<bool is_set, std::enable_if_t<!is_set, int>>
	inline std::pair<typename RedBlackTree_base<K, V, C, H>::Iterator, bool>
	RedBlackTree_base<K, V, C, H>::Insert(KeyType &&key, ValueType &&value)
	{
		const auto hash = this->m_algo(key);
		auto *node = this->m_allocator.allocate(1);
		std::allocator_traits<AllocatorType>::construct(this->m_allocator,
		                                                node,
		                                                std::forward<KeyType>(key), std::forward<ValueType>(value),
		                                                std::move(hash));
		std::unique_lock lck(this->m_lock);
		return this->Insert(node);
	}

	template<typename K, typename V, typename C, typename H>
	template<bool is_set, std::enable_if_t<is_set, int>>
	inline std::pair<typename RedBlackTree_base<K, V, C, H>::Iterator, bool>
	RedBlackTree_base<K, V, C, H>::Insert(KeyType &&key)
	{
		const auto hash = this->m_algo(key);
		auto *node = this->m_allocator.allocate(1);
		std::allocator_traits<AllocatorType>::construct(this->m_allocator,
		                                                node,
		                                                std::forward<KeyType>(key),
		                                                std::move(hash));
		std::unique_lock lck(this->m_lock);
		return this->Insert(node);
	}

	template<typename K, typename V, typename C, typename H>
	inline typename RedBlackTree_base<K, V, C, H>::SizeType RedBlackTree_base<K, V, C, H>::AverageHeight() const
	{
		return 2 * static_cast<std::size_t>(std::log2(this->m_size + 1));
	}

	template<typename K, typename V, typename C, typename H>
	inline typename RedBlackTree_base<K, V, C, H>::Iterator RedBlackTree_base<K, V, C, H>::Begin()
	{
		return Iterator(*this->m_root->LeftMost());
	}

	template<typename K, typename V, typename C, typename H>
	inline typename RedBlackTree_base<K, V, C, H>::ConstIterator RedBlackTree_base<K, V, C, H>::Begin() const
	{
		return ConstIterator(*this->m_root->LeftMost());
	}

	template<typename K, typename V, typename C, typename H>
	inline typename RedBlackTree_base<K, V, C, H>::Iterator RedBlackTree_base<K, V, C, H>::End()
	{
		return Iterator();
	}

	template<typename K, typename V, typename C, typename H>
	inline typename RedBlackTree_base<K, V, C, H>::ConstIterator RedBlackTree_base<K, V, C, H>::End() const
	{
		return ConstIterator();
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::Merge(RedBlackTree_base &&map)
	{
		std::unique_lock myLock(this->m_lock, std::defer_lock);
		std::unique_lock otherLock(map.m_lock, std::defer_lock);
		std::lock(myLock, otherLock);

		this->Merge(map.m_root);

		map.m_size = 0ULL;
		map.m_root = nullptr;
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::Merge(NodeType *node)
	{
		if(node == nullptr) {
			return;
		}

		auto *left = node->m_left;
		auto *right = node->m_right;

		node->m_left = node->m_right = node->m_parent = nullptr;
		node->m_color = NodeType::ColorType::RED;

		this->Merge(left);
		this->Merge(right);
		auto result = this->Insert(node);

		if(!result.second) {
			std::allocator_traits<AllocatorType>::destroy(this->m_allocator, node);
			this->m_allocator.deallocate(node, 1);
		}
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::Merge(const RedBlackTree_base &map)
	{
		std::unique_lock myLock(this->m_lock, std::defer_lock);
		std::shared_lock otherLock(map.m_lock, std::defer_lock);
		std::lock(myLock, otherLock);

		this->CopyMerge(map.m_root);
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::CopyMerge(const NodeType *node)
	{
		if(node == nullptr) {
			return;
		}

		auto *copy = this->m_allocator.allocate(1);

		if constexpr (IsSet) {
			std::allocator_traits<AllocatorType>::construct(this->m_allocator, copy, node->m_key, node->m_hash);
		} else {
			std::allocator_traits<AllocatorType>::construct(this->m_allocator,
			                                                copy,
			                                                node->m_key,
			                                                node->m_value,
			                                                node->m_hash);
		}

		const auto *left = std::as_const(node->m_left);
		const auto *right = std::as_const(node->m_right);

		this->CopyMerge(left);
		this->CopyMerge(right);
		auto result = this->Insert(copy);

		if(!result.second) {
			std::allocator_traits<AllocatorType>::destroy(this->m_allocator, copy);
			this->m_allocator.deallocate(copy, 1);
		}
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::Copy(const NodeType *node)
	{
		if(node == nullptr) {
			return;
		}

		auto *newNode = this->m_allocator.allocate(1);

		if constexpr (IsSet) {
			std::allocator_traits<AllocatorType>::construct(this->m_allocator, newNode, node->m_key, node->m_hash);
		} else {
			std::allocator_traits<AllocatorType>::construct(this->m_allocator,
			                                                newNode,
			                                                node->m_key,
			                                                node->m_value,
			                                                node->m_hash);
		}

		this->Copy(node->m_left);
		this->Copy(node->m_right);
		auto result = this->Insert(newNode);

		if(!result.second) {
			std::allocator_traits<AllocatorType>::destroy(this->m_allocator, newNode);
			this->m_allocator.deallocate(newNode, 1);
		}
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::Swap(NodeType *a, NodeType *b)
	{
		NodeType *new_p_parent = b->m_parent;
		NodeType *new_p_left = b->m_left;
		NodeType *new_p_right = b->m_right;
		NodeType **new_p_link = &this->m_root;

		if(a == nullptr || b == nullptr) {
			return;
		}

		if(b->m_parent) {
			new_p_link = b->IsLeftChild() ? &b->m_parent->m_left : &b->m_parent->m_right;
		}

		NodeType *new_q_parent = a->m_parent;
		NodeType *new_q_left = a->m_left;
		NodeType *new_q_right = a->m_right;
		NodeType **new_q_link = &this->m_root;

		if(a->m_parent) {
			new_q_link = a->IsLeftChild() ? &a->m_parent->m_left : &a->m_parent->m_right;
		}

		if(b->m_parent == a) {
			new_p_parent = b;
			new_p_link = nullptr;

			if(a->m_left == b) {
				new_q_left = a;
			} else {
				new_q_right = a;
			}
		} else if(a->m_parent == b) {
			new_q_parent = a;
			new_q_link = nullptr;

			if(b->m_left == a) {
				new_p_left = b;
			} else {
				new_p_right = b;
			}
		}

		a->m_parent = new_p_parent;
		a->m_left = new_p_left;

		if(a->m_left) {
			a->m_left->m_parent = a;
		}

		a->m_right = new_p_right;

		if(a->m_right) {
			a->m_right->m_parent = a;
		}

		if(new_p_link) {
			*new_p_link = a;
		}

		b->m_parent = new_q_parent;
		b->m_left = new_q_left;

		if(b->m_left) {
			b->m_left->m_parent = b;
		}

		b->m_right = new_q_right;

		if(b->m_right) {
			b->m_right->m_parent = b;
		}

		if(new_q_link) {
			*new_q_link = b;
		}

		std::swap(a->m_color, b->m_color);
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::Cleanup(boost::chrono::milliseconds timeout)
	{
		std::unique_lock l(this->m_lock);

		const auto now = ClockType::now();
		const auto tmo = now + timeout;
		bool done;

		do {
			done = this->RawCleanup();
		} while(ClockType::now() < tmo && !done);
	}

	template<typename K, typename V, typename C, typename H>
	inline bool RedBlackTree_base<K, V, C, H>::RawCleanup()
	{
		stl::list_head *carriage, *safety;
		const auto now = ClockType::now();
		using Millis = boost::chrono::milliseconds;
		std::size_t idx = 0UL;

		list_for_each_safe(carriage, safety, &this->m_tmo_queue) {
			if(idx >= CleanupBatchSize) {
				break;
			}

			auto node = stl::owner_of(carriage, &NodeType::m_entry);
			const auto age = now - node->m_created;

			if(boost::chrono::duration_cast<Millis>(age).count() < this->m_timeout) {
				return true;
			}

			this->Erase(node);
			--this->m_size;
			idx++;
		}

		return false;
	}

	template<typename K, typename V, typename C, typename H>
	inline void RedBlackTree_base<K, V, C, H>::Cleanup()
	{
		std::unique_lock l(this->m_lock);
		this->RawCleanup();
	}

	template<typename K, typename V, typename C, typename H>
	inline const typename RedBlackTree_base<K, V, C, H>::ValueType &
	RedBlackTree<K, V, C, H>::At(const typename Base::KeyType &key, typename Base::TickType now) const
	{
		auto iter = this->Find(key, now);

		if(iter == this->End()) {
			throw std::out_of_range(Base::KeyNotFound.data());
		}

		return *iter;
	}

	template<typename K, typename V, typename C, typename H>
	inline typename RedBlackTree_base<K, V, C, H>::ValueType &
	RedBlackTree<K, V, C, H>::At(const typename Base::KeyType &key, typename Base::TickType now)
	{
		auto iter = this->Find(key, now);

		if(iter == this->End()) {
			throw std::out_of_range(Base::KeyNotFound.data());
		}

		return *iter;
	}

	template<typename K, typename C, typename H>
	inline bool RedBlackTree<K, void, C, H>::Has(const typename Base::KeyType &key, typename Base::TickType now) const noexcept
	{
		return this->Find(key, now) != this->End();
	}
}
