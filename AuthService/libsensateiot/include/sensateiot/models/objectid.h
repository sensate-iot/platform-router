/*
 * MongoDB object ID wrapper.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <array>
#include <cstddef>
#include <iostream>

#include <boost/algorithm/hex.hpp>
#include <boost/multiprecision/cpp_int.hpp>

namespace sensateiot::models
{
	class ObjectId {
		typedef std::array<std::byte, 12> obj_id_t;

	public:
		static constexpr int ObjectIdSize = 12;
		static constexpr int BitSize = ObjectIdSize * 8;


		typedef boost::multiprecision::number<boost::multiprecision::cpp_int_backend<
				BitSize, // Minimum bits
				BitSize, // Maximum bits
				boost::multiprecision::unsigned_magnitude, // Magnitude
				boost::multiprecision::unchecked, // Checked
				void // Allocator
		>> ObjectIdType;

		constexpr ObjectId() = default;

		explicit ObjectId(const std::array<std::byte, ObjectIdSize> &bits)
		{
			boost::multiprecision::import_bits(this->m_objId, bits.begin(), bits.end());
		}

		explicit ObjectId(const std::uint8_t (&bits)[ObjectIdSize])
		{
			boost::multiprecision::import_bits(this->m_objId, std::begin(bits), std::end(bits));
//			std::cout << std::hex << "Object ID: " << this->m_objId << std::endl;
//			std::cout << std::dec << "Object ID size: " << sizeof(ObjectId) << std::endl;
		}

		explicit ObjectId(const std::string &hex)
		{
			std::uint8_t bits[12];

			auto hash = boost::algorithm::unhex(hex);
			std::move(hash.begin(), hash.end(), bits);
			boost::multiprecision::import_bits(this->m_objId, std::begin(bits), std::end(bits));
		}

		[[nodiscard]]
		int compare(const ObjectId &other) const
		{
			return this->m_objId.compare(other.m_objId);
		}

		[[nodiscard]]
		constexpr const ObjectIdType& Value() const
		{
			return this->m_objId;
		}

		[[nodiscard]]
		bool operator==(const ObjectId& other) const
		{
			return this->m_objId.compare(other.m_objId) == 0;
		}

		[[nodiscard]]
		std::string ToString() const
		{
			std::stringstream sstream;

			sstream << std::hex << this->m_objId;
			return sstream.str();
		}

	private:
		ObjectIdType m_objId;
	};
}
