#
# CMakeLists for libauthservice.
#

SET(SOURCES
	models/apikey.cpp
	models/user.cpp
	models/sensor.cpp
)

include_directories(
	${PROJECT_SOURCE_DIR}/libauthorize/include
	${PROJECT_SOURCE_DIR}/external/ctre/single-header
)

add_library(authorize SHARED ${SOURCES})