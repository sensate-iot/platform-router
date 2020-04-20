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

	mqtt/mqttclient.cpp
	mqtt/mqttcallback.cpp
)

add_library(authorization SHARED ${SOURCES})
target_link_libraries(authorization ${CONAN_LIBS})