CMAKE_MINIMUM_REQUIRED(VERSION 3.6)
PROJECT(AuthService)

SET(CMAKE_CXX_STANDARD 17)
SET(CMAKE_C_STANDARD 11)
set(CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS 1)

# Config options
SET(CONFIG_BUILD_TESTS False CACHE BOOL "Build unit tests.")

SET(AUTHSERVICE_SOURCES
	${PROJECT_SOURCE_DIR}/source/main.c
	${PROJECT_SOURCE_DIR}/source/application.cpp
)

if(MSVC)
	set(AUTHSERVICE_SOURCES
		${AUTHSERVICE_SOURCES}
		${PROJECT_SOURCE_DIR}/source/getopt_win32.c
	)

	set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} /ignore:4099")
	set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} /ignore:4099")
	set(CMAKE_STATIC_LINKER_FLAGS "${CMAKE_STATIC_LINKER_FLAGS} /ignore:4099")
endif()

INCLUDE(${CMAKE_BINARY_DIR}/conanbuildinfo.cmake)
conan_basic_setup()

if(NOT MSVC)
	SET(WARNING_FLAGS "\
	-Wconversion \
	-Wnull-dereference \
	-Wdouble-promotion \
	-Wduplicated-cond \
	-Wduplicated-branches \
	-Wlogical-op \
	-Wnon-virtual-dtor")
	SET(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} ${WARNING_FLAGS} -mtune=generic -ffast-math")
	SET(CMAKE_C_FLAGS_RELEASE "${CMAKE_C_FLAGS_RELEASE} -mtune=generic -ffast-math")
	SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} ${WARNING_FLAGS} -Wall -Wno-parentheses")
	SET(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -Wall -Wno-parentheses")
endif()

SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -D_SILENCE_CXX17_ITERATOR_BASE_CLASS_DEPRECATION_WARNING")
SET(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -D_SILENCE_CXX17_ITERATOR_BASE_CLASS_DEPRECATION_WARNING")

include_directories(
	${PROJECT_SOURCE_DIR}/external/ctre/single-header
	${PROJECT_SOURCE_DIR}/external/json
	${PROJECT_SOURCE_DIR}/include
	${PROJECT_SOURCE_DIR}/libsensateiot/include
	SYSTEM INTERFACE ${CONAN_INCLUDE_DIRS}
)

add_subdirectory(libsensateiot)

ADD_EXECUTABLE(auth-service ${AUTHSERVICE_SOURCES})
TARGET_LINK_LIBRARIES(auth-service sensateiot ${CONAN_LIBS})

if (MSVC)
	SET_TARGET_PROPERTIES(auth-service PROPERTIES LINK_FLAGS /ignore:4099)
endif()

if (UNIX)
	INSTALL(DIRECTORY ${PROJECT_BINARY_DIR}/libdist/ DESTINATION lib)
endif()

INSTALL(TARGETS auth-service DESTINATION bin)

if (CONFIG_BUILD_TESTS)
	add_subdirectory(tests)
endif()
