#
# Tests CMakeLists.
#
# Author: Michel Megens
# Email:  michel@michelmegens.net
#

ADD_EXECUTABLE(messageservice_test messageservice_test.cpp)
TARGET_LINK_LIBRARIES(messageservice_test sensateiot ${CONAN_LIBS})

ADD_EXECUTABLE(validator_test validator_test.cpp)
TARGET_LINK_LIBRARIES(validator_test sensateiot ${CONAN_LIBS})

ADD_EXECUTABLE(sha256_test sha256_test.cpp)
TARGET_LINK_LIBRARIES(sha256_test sensateiot ${CONAN_LIBS})

ADD_EXECUTABLE(map_test map_test.cpp)
TARGET_LINK_LIBRARIES(map_test sensateiot ${CONAN_LIBS})

ADD_EXECUTABLE(zip_test zip_test.cpp)
TARGET_LINK_LIBRARIES(zip_test sensateiot ${CONAN_LIBS})