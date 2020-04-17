#
# CMakeLists for libauthservice.
#

SET(SOURCES
	models/apikey.cpp
	models/user.cpp
	models/sensor.cpp
)

include_directories(
	${PROJECT_SOURCE_DIR}/libauthorization/include
)

add_library(authorize SHARED ${SOURCES})