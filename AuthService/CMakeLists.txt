CMAKE_MINIMUM_REQUIRED(VERSION 3.6)
PROJECT(AuthService)

include (${PROJECT_SOURCE_DIR}/cmake/util.cmake)
include (${PROJECT_SOURCE_DIR}/cmake/flags.cmake)

INCLUDE(${CMAKE_BINARY_DIR}/conanbuildinfo.cmake)
conan_basic_setup()

# Config options
SET(CONFIG_BUILD_TESTS False CACHE BOOL "Build unit tests.")

SET(AUTHSERVICE_SOURCES
	${PROJECT_SOURCE_DIR}/source/main.c
	${PROJECT_SOURCE_DIR}/source/application.cpp
	${PROJECT_SOURCE_DIR}/source/getopt.c
	${PROJECT_SOURCE_DIR}/source/configparser.cpp
)

SET(AUTHSERVICE_HEADERS
	${PROJECT_SOURCE_DIR}/include/sensateiot/application.h
	${PROJECT_SOURCE_DIR}/include/sensateiot/configparser.h
	${PROJECT_SOURCE_DIR}/include/sensateiot/getopt.h
)

include_directories(
	${PROJECT_SOURCE_DIR}/include
	${PROJECT_SOURCE_DIR}/libsensateiot/include
	SYSTEM ${PROJECT_SOURCE_DIR}/external/json
	SYSTEM ${PROJECT_SOURCE_DIR}/external/hopscotch-map/include
	SYSTEM INTERFACE ${CONAN_INCLUDE_DIRS}
)

add_subdirectory(libsensateiot)

ADD_EXECUTABLE(auth-service ${AUTHSERVICE_SOURCES} ${AUTHSERVICE_HEADERS})
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
