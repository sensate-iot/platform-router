#
# Tests CMakeLists.
#

ADD_EXECUTABLE(messageservice_test messageservice_test.cpp)
TARGET_LINK_LIBRARIES(messageservice_test authorization ${CONAN_LIBS})
