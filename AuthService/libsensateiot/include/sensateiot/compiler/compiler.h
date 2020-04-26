/*
 * Generic compiler header.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#ifdef WIN32
#include <sensateiot/compiler/compiler-msvc.h>
#else
#endif

#ifndef DLL_EXPORT
#define DLL_EXPORT
#endif
