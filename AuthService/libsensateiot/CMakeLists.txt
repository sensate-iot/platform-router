#
# CMakeLists for libauthservice.
#

# Lib sensateiot source / header files

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
	util/mongodbclientpool.cpp
	util/mongodbclient.cpp

	data/datacache.cpp

	services/userrepository.cpp
	services/abstractuserrepository.cpp
	services/abstractapikeyrepository.cpp
	services/apikeyrepository.cpp
	services/sensorrepository.cpp
	services/abstractsensorrepository.cpp

	mqtt/messageservice.cpp
	mqtt/measurementhandler.cpp
	mqtt/mqttcallback.cpp
	mqtt/mqttclient.cpp
	mqtt/mqttinternalcallback.cpp
)

set(GENERIC_HEADERS
	libsensateiot/include/auth.h
	libsensateiot/include/config/config.h
	libsensateiot/include/config/database.h
	libsensateiot/include/config/logging.h
	libsensateiot/include/config/mqtt.h
	libsensateiot/include/config/mqttbroker.h
	libsensateiot/include/sensateiot/compiler/compiler-msvc.h
	libsensateiot/include/sensateiot/compiler/compiler.h
	libsensateiot/include/sensateiot/models/apikey.h
	libsensateiot/include/sensateiot/models/sensor.h
	libsensateiot/include/sensateiot/models/user.h
	libsensateiot/include/sensateiot/mqtt/imqttclient.h
	libsensateiot/include/sensateiot/mqtt/messageservice.h
	libsensateiot/include/sensateiot/mqtt/measurementhandler.h
	libsensateiot/include/sensateiot/mqtt/mqttcallback.h
	libsensateiot/include/sensateiot/mqtt/mqttclient.h
	libsensateiot/include/sensateiot/mqtt/mqttinternalcallback.h
	libsensateiot/include/sensateiot/services/abstractapikeyrepository.h
	libsensateiot/include/sensateiot/services/abstractuserrepository.h
	libsensateiot/include/sensateiot/services/abstractsensorrepository.h
	libsensateiot/include/sensateiot/services/apikeyrepository.h
	libsensateiot/include/sensateiot/services/userrepository.h
	libsensateiot/include/sensateiot/services/sensorrepository.h
	libsensateiot/include/sensateiot/stl/referencewrapper.h
	libsensateiot/include/sensateiot/util/log.h
	libsensateiot/include/sensateiot/util/mongodbclientpool.h
	libsensateiot/include/sensateiot.h
)

FUNCTION( prepend_path SOURCE_FILES INC_PATH )
	FOREACH( SOURCE_FILE ${${SOURCE_FILES}} )
		SET( MODIFIED ${MODIFIED} ${INC_PATH}/${SOURCE_FILE} )
	ENDFOREACH()
	SET( ${SOURCE_FILES} ${MODIFIED} PARENT_SCOPE )
ENDFUNCTION()

SET(INCLUDE_DIR ${PROJECT_SOURCE_DIR})
prepend_path(GENERIC_HEADERS ${INCLUDE_DIR})

add_library(sensateiot SHARED ${SOURCES} ${GENERIC_HEADERS})
target_link_libraries(sensateiot ${CONAN_LIBS})

if (MSVC)
	SET_TARGET_PROPERTIES(sensateiot PROPERTIES LINK_FLAGS /ignore:4099)
endif()

install(TARGETS sensateiot
	EXPORT SnappyTargets
	# RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR} # DESTINATION error
	RUNTIME DESTINATION bin ${CMAKE_INSTALL_BINDIR} # should add bin or other dir
	LIBRARY DESTINATION lib ${CMAKE_INSTALL_LIBDIR}
	# ARCHIVE DESTINATION ${CMAKE_INSTALL_LIBDIR # DESTINATION error
	ARCHIVE DESTINATION lib ${CMAKE_INSTALL_LIBDIR} # should add lib
	)