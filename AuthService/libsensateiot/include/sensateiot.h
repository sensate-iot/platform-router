/*
 * Main header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#ifdef __cplusplus

#include <type_traits>
#include <cstdint>

#include <sensateiot/compiler/compiler.h>

namespace sensateiot::auth
{
	typedef std::uint8_t byte_t;
	typedef unsigned long word_t;
}

#ifndef ns_base
#define ns_base
#endif

#endif

#define UNUSED(x) (void)x;
