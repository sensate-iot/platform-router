CMAKE_MINIMUM_REQUIRED(VERSION 2.8.12)
PROJECT(AuthService)

SET(CMAKE_CXX_STANDARD 17)
SET(CMAKE_C_STANDARD 11)

INCLUDE(${CMAKE_BINARY_DIR}/conanbuildinfo.cmake)
conan_basic_setup()

add_subdirectory(libauthorization)

ADD_EXECUTABLE(auth-service source/main.c)
TARGET_LINK_LIBRARIES(auth-service ${CONAN_LIBS} authorize)

