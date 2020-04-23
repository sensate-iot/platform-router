#
# CMakeLists for libauthservice.
#

SET(SOURCES
	models/apikey.cpp
	models/user.cpp
	models/sensor.cpp

	config/config.cpp
	config/database.cpp
	config/mqttbroker.cpp
	config/mqtt.cpp
	config/logging.cpp

	util/log.cpp

	mqtt/mqttcallback.cpp
	mqtt/mqttclient.cpp
	mqtt/mqttinternalcallback.cpp
)

add_library(authorization SHARED ${SOURCES})
target_link_libraries(authorization ${CONAN_LIBS})

if (MSVC)
	SET_TARGET_PROPERTIES(authorization PROPERTIES LINK_FLAGS /ignore:4099)
endif()
