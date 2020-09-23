#
# Tests CMakeLists.
#
# Author: Michel Megens
# Email:  michel@michelmegens.net
#

if(CONAN_CATCH2_ROOT_DEBUG)
  include(${CONAN_CATCH2_ROOT_DEBUG}/lib/cmake/Catch2/Catch.cmake)
else()
  include(${CONAN_CATCH2_ROOT}/lib/cmake/Catch2/Catch.cmake)
endif()

ADD_EXECUTABLE(messageservice_test messageservice_test.cpp)
TARGET_LINK_LIBRARIES(messageservice_test sensateiot ${CONAN_LIBS})

ADD_EXECUTABLE(validator_test validator_test.cpp)
TARGET_LINK_LIBRARIES(validator_test sensateiot ${CONAN_LIBS})

ADD_EXECUTABLE(sha256_test sha256_test.cpp)
TARGET_LINK_LIBRARIES(sha256_test sensateiot ${CONAN_LIBS})

ADD_EXECUTABLE(memorycache_test memorycache_test.cpp)
TARGET_LINK_LIBRARIES(memorycache_test sensateiot ${CONAN_LIBS})

ADD_EXECUTABLE(zip_test zip_test.cpp)
TARGET_LINK_LIBRARIES(zip_test sensateiot ${CONAN_LIBS})

# Unit tests

ADD_LIBRARY(catch_main STATIC unit_tests_main.cpp)
TARGET_LINK_LIBRARIES(catch_main sensateiot ${CONAN_LIBS})

ADD_EXECUTABLE(memorycache_unit-tests
	memorycache-generic_unit-tests.cpp
	memorycache-add_unit-tests.cpp
	memorycache-lookup_unit-tests.cpp
	memorycache-update_unit-tests.cpp
	memorycache-remove_unit-tests.cpp
)
TARGET_LINK_LIBRARIES(memorycache_unit-tests PRIVATE sensateiot catch_main)

catch_discover_tests(
  memorycache_unit-tests
  TEST_PREFIX
  "unittests."
  EXTRA_ARGS
  -s
  --reporter=xml
  --out=tests.xml)
