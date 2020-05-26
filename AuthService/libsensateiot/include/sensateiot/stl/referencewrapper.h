/*
 * Reference Wrapper header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

/// @file referencewrapper.h Ref wrapper header.

#pragma once

#include <cassert>
#include <type_traits>

namespace sensateiot::stl
{
	/**
	 * @ingroup stl
	 * @brief Reference wrapper header.
	 * @tparam T Reference type.
	 */
	template <typename T>
	class ReferenceWrapper {
	public:
		typedef T type; //!< Reference type.

		/**
		 * @brief Construct a new reference wrapper.
		 */
		constexpr ReferenceWrapper() noexcept : _value(nullptr)
		{ }

		/**
		 * @brief Construct a new reference wrapper.
		 * @param value Initial value.
		 */
		explicit constexpr ReferenceWrapper(type& value) noexcept : _value(&value)
		{ }

		/**
		 * @brief Copy construct a new reference wrapper.
		 * @param other Reference wrapper to copy.
		 */
		constexpr ReferenceWrapper(const ReferenceWrapper& other) noexcept : _value(other._value)
		{ }

		/**
		 * @brief Move construct a new reference wrapper.
		 * @param other Reference wrapper to move.
		 */
		constexpr ReferenceWrapper(ReferenceWrapper&& other) noexcept : _value(other._value)
		{
			other._value = nullptr;
		}

		/**
		 * @brief Value assignment operator.
		 * @param other Value to assign.
		 */
		constexpr ReferenceWrapper& operator=(type& other)
		{
			this->_value = &other;
			return *this;
		}

		/**
		 * @brief Copy assignment operator.
		 * @param other Other reference wrapper.
		 * @return Reference to \p *this.
		 */
		constexpr ReferenceWrapper& operator=(const ReferenceWrapper& other)
		{
			if(this == &other) {
				return *this;
			}

			this->_value = other._value;
			return *this;
		}

		/**
		 * @brief Copy assignment operator.
		 * @param other Other reference wrapper.
		 * @return Reference to \p *this.
		 */
		constexpr ReferenceWrapper& operator=(ReferenceWrapper&& other) noexcept
		{
			this->_value = other._value;
			other._value = nullptr;

			return *this;
		}

		bool operator==(const ReferenceWrapper& other) const
		{
			if(this->_value == other._value) {
				return true;
			}

			if(this->_value == nullptr || other._value == nullptr) {
				return false;
			}

			return this->get() == other.get();
		}

		bool operator!=(const ReferenceWrapper& other) const
		{
			if(this->_value == other._value) {
				return false;
			}

			if(this->_value == nullptr || other._value == nullptr) {
				return true;
			}

			return this->get() != other.get();
		}

		/**
		 * @brief Boolean conversion operator.
		 * @return True or false based on the validity of \p *this.
		 */
		constexpr operator bool() const
		{
			return this->_value != nullptr;
		}

		/**
		 * @brief Get the underlying reference.
		 * @return The underlying reference.
		 */
		constexpr type& get() const noexcept
		{
			assert(this->_value != nullptr);
			return *this->_value;
		}

		/**
		 * @brief Get the underlying reference.
		 * @return The underlying reference.
		 */
		constexpr type* get_ptr() const noexcept
		{
			assert(this->_value != nullptr);
			return this->_value;
		}

		/**
		 * @brief Get the underlying reference.
		 * @return The underlying reference.
		 */
		constexpr operator type&() const noexcept
		{
			assert(this->_value != nullptr);
			return this->get();
		}

		/**
		 * @brief Get the underlying reference.
		 * @return The underlying reference.
		 */
		constexpr type& operator*()
		{
			return *this->_value;
		}

		/**
		 * @brief Access the underlying reference.
		 * @return The underlying object.
		 */
		constexpr type* operator->() const noexcept
		{
			assert(this->_value != nullptr);
			return this->_value;
		}

	private:
		type* _value;
	};

	/**
	 * @brief Denotes a reference should be taken to a variable.
	 * @tparam T Reference type.
	 * @param ref Object wrap into a ReferenceWrapper.
	 * @return The reference wrapper object around \p ref.
	 */
	template <typename T>
	constexpr ReferenceWrapper<T> MakeRef(T& ref) noexcept
	{
		return ReferenceWrapper<T>(ref);
	}

	/**
	 * @brief Denotes a const reference should be taken to a variable.
	 * @tparam T Reference type.
	 * @param ref Object wrap into a const ReferenceWrapper.
	 * @return The reference wrapper object around \p ref.
	 */
	template <typename T>
	constexpr ReferenceWrapper<typename std::add_const<T>::type> MakeCRef(const T& ref) noexcept
	{
		return ReferenceWrapper<typename std::add_const<T>::type>(ref);
	}
}
