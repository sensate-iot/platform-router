cmake_minimum_required(VERSION 2.8.12)
project(AuthService)

add_definitions("-std=c++17")

include(${CMAKE_BINARY_DIR}/conanbuildinfo.cmake)
conan_basic_setup()

add_executable(auth-service source/main.c)
target_link_libraries(auth-service ${CONAN_LIBS})
