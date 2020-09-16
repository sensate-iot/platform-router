/*
 * Red-black tree definition.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <boost/chrono.hpp>
#include <boost/uuid/uuid.hpp>
#include <boost/multiprecision/cpp_int.hpp>

#include <sensateiot/stl/clist.h>
#include <sensateiot/stl/smallvector.h>
#include <sensateiot/util/log.h>
#include <sensateiot/models/objectid.h>

#include "redblacktreenode.h"
#include "redblacktreeiterator.h"

#include <cstddef>
#include <memory>
#include <type_traits>
#include <utility>
#include <shared_mutex>
#include <mutex>
#include <atomic>
#include <cmath>
#include <condition_variable>

namespace sensateiot::detail
{
	namespace traits
	{
		template<typename T>
		struct is_callable {
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
			static constexpr bool value = sizeof(test<Derived>(nullptr)) == sizeof(yes);
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
		template <typename T>
		struct MurmurHash3 {
			typedef std::uint32_t HashType;
			HashType operator()(const T* array, std::size_t size) const;

		private:
			static constexpr std::uint32_t fmix32(HashType h)
			{
				h ^= h >> 16;
				h *= 0x85ebca6b;
				h ^= h >> 13;
				h *= 0xc2b2ae35;
				h ^= h >> 16;

				return h;
			}

			static constexpr std::uint32_t rotl32(std::uint32_t x, std::uint32_t r)
			{
				return ((x) << (r)) | ((x) >> (32U - (r)));
			}

			static constexpr HashType seed = 0x8FE3C9A1;
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
		typedef algo::MurmurHash3<std::uint8_t>::HashType HashType;

		HashType operator()(const std::string& value) const
		{
			const auto* data = reinterpret_cast<const std::uint8_t*>(value.data());
			return this->m_algo(data, value.size());
		}

	private:
		algo::MurmurHash3<std::uint8_t> m_algo;
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

	template <typename T>
	struct Compare {
		int operator()(const T& a, const T& b) const
		{
			if(a < b) {
				return -1;
			} else if(a > b) {
				return 1;
			}

			return 0;
		}
	};

	template <>
	struct Compare<models::ObjectId> {
		int operator()(const models::ObjectId& a, const models::ObjectId& b) const
		{
			return a.compare(b);
		}
	};

	template <>
	struct Compare<std::string> {
		int operator()(const std::string& a, const std::string& b) const
		{
			return a.compare(b);
		}
	};

	struct SetType {};

	template <typename K, typename V, typename C, typename H>
	class RedBlackTree_base {
		static constexpr bool HashAlgorithmValid()
		{
			return traits::is_callable<H>::value && traits::TestHashAlgorithm<H>::Value;
		}

		static_assert(std::is_move_constructible_v<K>, "K should be move constructable!");
		static_assert(std::is_move_constructible_v<V>, "V should be move constructable!");

		static_assert(std::is_copy_constructible_v<K>, "K should be copy constructable!");
		static_assert(std::is_copy_constructible_v<V>, "V should be copy constructable!");

		static_assert(std::is_move_assignable_v<K>, "K should be move assignable!");
		static_assert(std::is_move_assignable_v<V>, "V should be move assignable!");

		static_assert(std::is_copy_assignable_v<K>, "K should be copy assignable!");
		static_assert(std::is_copy_assignable_v<V>, "V should be copy assignable!");

		static_assert(traits::is_callable<C>::value, "C should be callable!");
		static_assert(HashAlgorithmValid(), "H should be callable and contain the HashType type definition!");

	public:
		typedef std::size_t SizeType;
		typedef C ComparatorType;
		typedef K KeyType;
		typedef V ValueType;

		static constexpr SizeType DefaultTimeout = 30 * 60 * 1000; // 30 minutes in millis
		static constexpr bool IsSet = std::is_same_v<SetType, V>;

		typedef boost::chrono::high_resolution_clock ClockType;
		typedef ClockType::time_point TickType;
		typedef std::allocator<RedBlackTreeNode<K,V,ClockType, ComparatorType, IsSet>> AllocatorType;

		typedef RedBlackTreeIterator<K,V,C,H,true,IsSet> ConstIterator;
		typedef RedBlackTreeIterator<K,V,C,H,false,IsSet> Iterator;

		typedef RedBlackTreeIterator<K,V,C,H,true,true> ConstKeyIterator;
		typedef RedBlackTreeIterator<K,V,C,H,false,true> KeyIterator;

		explicit RedBlackTree_base(long timeout = DefaultTimeout);
		explicit RedBlackTree_base(boost::chrono::milliseconds tmo);

		RedBlackTree_base(const RedBlackTree_base& other);
		RedBlackTree_base(RedBlackTree_base&& other) noexcept;

		virtual ~RedBlackTree_base();

		RedBlackTree_base& operator=(const RedBlackTree_base& rhs);
		RedBlackTree_base& operator=(RedBlackTree_base&& rhs) noexcept;

		void Clear();

		Iterator Find(const KeyType& key, TickType now = ClockType::now());
		ConstIterator Find(const KeyType& key, TickType now = ClockType::now()) const;

		bool Empty() const;
		SizeType Size() const;

		void Merge(const RedBlackTree_base& map);
		void Merge(RedBlackTree_base&& map);
		bool Contains(const KeyType& key) const;
		template <typename... Args>
		std::pair<Iterator, bool> Emplace(Args&&... args);

		template <bool is_set = IsSet, std::enable_if_t<is_set, int> = 0>
		std::pair<Iterator, bool> Insert(const KeyType& key);

		template <bool is_set = IsSet, std::enable_if_t<!is_set, int> = 0>
		std::pair<Iterator, bool> Insert(const KeyType& key, const ValueType& value);

		template <bool is_set = IsSet, std::enable_if_t<!is_set, int> = 0>
		std::pair<Iterator, bool> Insert(Iterator iter, const ValueType& value);

		template <bool is_set = IsSet, std::enable_if_t<!is_set, int> = 0>
		std::pair<Iterator, bool> Insert(Iterator iter, ValueType&& value);

		template <bool is_set = IsSet, std::enable_if_t<!is_set, int> = 0>
		ValueType& operator[](const KeyType& k);
		template <bool is_set = IsSet, std::enable_if_t<!is_set, int> = 0>
		const ValueType& operator[](const KeyType& k) const;

		template <bool is_set = IsSet, std::enable_if_t<is_set, int> = 0>
		bool operator[](const KeyType& k) const;

		Iterator End();
		ConstIterator End() const;

		Iterator Begin();
		ConstIterator Begin() const;

		bool operator==(const RedBlackTree_base& other) const;
		bool operator!=(const RedBlackTree_base& other) const;

		void Erase(Iterator pos);
		void Erase(const KeyType& key);
		void Erase(Iterator first, Iterator last);
		void Cleanup();
		void Cleanup(boost::chrono::milliseconds timeout);

		bool Validate() const;
		SizeType AverageHeight() const;

	protected:
		typedef RedBlackTreeNode<KeyType, ValueType, ClockType, ComparatorType, IsSet> NodeType;
		typedef H HashAlgorithmType;
		typedef typename HashAlgorithmType::HashType HashType;
		typedef std::condition_variable_any ConditionType;

		AllocatorType m_allocator;
		mutable std::shared_mutex m_lock;

		static constexpr std::string_view KeyNotFound = "Key not found";

		void Copy(const RedBlackTree_base& other);
		void Move(RedBlackTree_base& other) noexcept;
		void Chop(NodeType* node) noexcept;

	private:
		NodeType* m_root;
		SizeType m_size;
		std::time_t m_timeout;
		HashAlgorithmType m_algo;
		ComparatorType m_compare;
		stl::list_head m_tmo_queue;

		static constexpr int CleanupBatchSize = 10;

		[[nodiscard]]
		std::pair<bool, ConstIterator> RawFind(const KeyType &key, TickType now) const noexcept;

		bool RawCleanup();
		void RotateLeft(NodeType* node);
		void RotateRight(NodeType* node);
		void FixInsert(NodeType* node);
		void DeleteFix(NodeType* node);
		void Erase(NodeType* v);
		std::pair<bool,SizeType> Validate(const NodeType* node) const;
		void Swap(NodeType* a, NodeType* b);

		template <bool is_set = IsSet, std::enable_if_t<!is_set, int> = 0>
		std::pair<Iterator, bool> Insert(KeyType&& key, ValueType&& value);

		template <bool is_set = IsSet, std::enable_if_t<is_set, int> = 0>
		std::pair<Iterator, bool> Insert(KeyType&& key);

		std::pair<Iterator, bool> Insert(NodeType* node);
		void Merge(NodeType* node);
		void CopyMerge(const NodeType* node);
		void Copy(const NodeType* node);

		friend class RedBlackTreeIterator<K,V,C,H,true,IsSet>;
		friend class RedBlackTreeIterator<K,V,C,H,false,IsSet>;

		friend class RedBlackTreeIterator<K,V,C,H,true,true>;
		friend class RedBlackTreeIterator<K,V,C,H,false,true>;
	};

	template <typename K, typename V, typename C, typename H>
	class RedBlackTree : public RedBlackTree_base<K, V, C, H> {
		friend class RedBlackTreeIterator<K,V,C,H,true,false>;
		friend class RedBlackTreeIterator<K,V,C,H,false,false>;

		friend class RedBlackTreeIterator<K,V,C,H,true,true>;
		friend class RedBlackTreeIterator<K,V,C,H,false,true>;

		typedef RedBlackTree_base<K, V, C, H> Base;

	protected:
		static constexpr typename Base::SizeType DefaultTimeout = Base::DefaultTimeout;

	public:
		explicit RedBlackTree(long timeout = DefaultTimeout) : RedBlackTree_base<K,V,C,H>(timeout) {}

		const typename Base::ValueType& At(const typename Base::KeyType& key, typename Base::TickType now = Base::ClockType::now()) const;
		typename Base::ValueType& At(const typename Base::KeyType& key, typename Base::TickType now = Base::ClockType::now());
	};

	template <typename K, typename C, typename H>
	class RedBlackTree<K, void, C, H> : public RedBlackTree_base<K, SetType, C, H> {
		friend class RedBlackTreeIterator<K,SetType,C,H,true,true>;
		friend class RedBlackTreeIterator<K,SetType,C,H,false,true>;

		typedef RedBlackTree_base<K, SetType, C, H> Base;


	protected:
		static constexpr typename Base::SizeType DefaultTimeout = Base::DefaultTimeout;

	public:
		explicit RedBlackTree(long timeout = DefaultTimeout) : RedBlackTree_base<K,SetType,C,H>(timeout) {}
		bool Has(const typename Base::KeyType& key, typename Base::TickType now = Base::ClockType::now()) const noexcept;
	};
}

#include "redblacktree.hpp"
