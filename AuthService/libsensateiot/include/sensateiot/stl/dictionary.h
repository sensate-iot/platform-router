/*
 * Concurrent STL dictionary.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <boost/unordered_map.hpp>

#include <shared_mutex>
#include <utility>

namespace sensateiot::stl
{
	template <typename K, typename V>
	class Dictionary {
	public:
		typedef K KeyType;
		typedef V ValueType;
		
		template <typename Func>
		auto Process(const KeyType& key, Func&& f) const
		{
			std::shared_lock l(this->m_mtx);
			auto& ref = this->m_values.at(key);
			return f(ref);
		}

		void Insert(const KeyType& key, const ValueType& value)
		{
			std::unique_lock l(this->m_mtx);
			this->m_values.insert_or_assign(key, value);
		}

		const ValueType& At(const KeyType& key) const
		{
			return this->m_values.at(key);
		}
		
		ValueType& At(const KeyType& key)
		{
			return this->m_values.at(key);
		}
		
		const ValueType& operator[](const KeyType& key) const
		{
			return this->m_values.at(key);
		}
		
		ValueType& operator[](const KeyType& key)
		{
			return this->m_values.at(key);
		}

	private:
		mutable std::shared_mutex m_mtx;
		boost::unordered_map<KeyType, ValueType> m_values;
	};
}
