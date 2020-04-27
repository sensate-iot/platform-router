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

	services/userrepository.cpp
	services/abstractuserrepository.cpp

	mqtt/messageservice.cpp
	mqtt/measurementhandler.cpp
	mqtt/mqttcallback.cpp
	mqtt/mqttclient.cpp
	mqtt/mqttinternalcallback.cpp
)

add_library(sensateiot SHARED ${SOURCES})
target_link_libraries(sensateiot ${CONAN_LIBS})

if (MSVC)
	SET_TARGET_PROPERTIES(sensateiot PROPERTIES LINK_FLAGS /ignore:4099)
endif()

install(TARGETS sensateiot LIBRARY DESTINATION lib)
