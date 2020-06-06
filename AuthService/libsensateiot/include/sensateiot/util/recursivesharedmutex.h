/*
 * Shared mutex with support for recursion.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

#pragma once

#include <atomic>
#include <thread>
#include <shared_mutex>

namespace sensateiot::util
{
	class RecursiveSharedMutex : public std::shared_mutex {
	public:
		void lock();
		void unlock();

	private:
		std::atomic<std::thread::id> m_owner;
		int m_count;
	};
}
