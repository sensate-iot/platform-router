#
# Tests CMakeLists.
#

ADD_EXECUTABLE(messageservice_test messageservice_test.cpp)
TARGET_LINK_LIBRARIES(messageservice_test sensateiot ${CONAN_LIBS})

ADD_EXECUTABLE(mongdbclient_test mongdbclient_test.cpp)
TARGET_LINK_LIBRARIES(mongdbclient_test sensateiot ${CONAN_LIBS})

ADD_EXECUTABLE(validator_test validator_test.cpp)
TARGET_LINK_LIBRARIES(validator_test sensateiot ${CONAN_LIBS})

ADD_EXECUTABLE(sha256_test sha256_test.cpp)
TARGET_LINK_LIBRARIES(sha256_test sensateiot ${CONAN_LIBS})

ADD_EXECUTABLE(map_test map_test.cpp)
TARGET_LINK_LIBRARIES(map_test sensateiot ${CONAN_LIBS})

ADD_EXECUTABLE(messageserviceintegration_test messageserviceintegration_test.cpp)
TARGET_LINK_LIBRARIES(messageserviceintegration_test sensateiot ${CONAN_LIBS})