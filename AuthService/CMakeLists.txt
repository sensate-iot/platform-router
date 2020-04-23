CMAKE_MINIMUM_REQUIRED(VERSION 3.6)
PROJECT(AuthService)

SET(CMAKE_CXX_STANDARD 17)
SET(CMAKE_C_STANDARD 11)
set(CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS 1)

SET(AUTHSERVICE_SOURCES
	${PROJECT_SOURCE_DIR}/source/main.c
	${PROJECT_SOURCE_DIR}/source/application.cpp
)

if(MSVC)
	set(AUTHSERVICE_SOURCES
		${AUTHSERVICE_SOURCES}
		${PROJECT_SOURCE_DIR}/source/getopt_win32.c
	)
endif()

INCLUDE(${CMAKE_BINARY_DIR}/conanbuildinfo.cmake)
conan_basic_setup()

include_directories(
	${PROJECT_SOURCE_DIR}/external/ctre/single-header
	${PROJECT_SOURCE_DIR}/external/json
	${PROJECT_SOURCE_DIR}/include
	${PROJECT_SOURCE_DIR}/libauthorization/include
)

add_subdirectory(libauthorization)

ADD_EXECUTABLE(auth-service ${AUTHSERVICE_SOURCES})
TARGET_LINK_LIBRARIES(auth-service authorization ${CONAN_LIBS})

if (MSVC)
	SET_TARGET_PROPERTIES(auth-service PROPERTIES LINK_FLAGS /ignore:4099)
endif()

