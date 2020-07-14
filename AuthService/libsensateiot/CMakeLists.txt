#
# CMakeLists for libauthservice.
#

# Lib sensateiot source / header files

SET(SOURCES
	models/apikey.cpp
	models/user.cpp
	models/sensor.cpp
	models/rawmeasurement.cpp

	config/config.cpp
	config/database.cpp
	config/mqttbroker.cpp
	config/mqtt.cpp
	config/logging.cpp

	util/log.cpp
	util/mongodbclientpool.cpp
	util/mongodbclient.cpp
	util/sha256.c
	util/sha256.cpp
	util/protobuf.cpp
	util/base64.cpp

	httpd/httpserver.cpp
	httpd/httpsession.cpp
	httpd/httplistener.cpp
	httpd/httpresponse.cpp
	httpd/httprequest.cpp

	http/statushandler.cpp
	http/measurementhandler.cpp
	http/abstracthandler.cpp

	services/userrepository.cpp
	services/abstractuserrepository.cpp
	services/abstractapikeyrepository.cpp
	services/apikeyrepository.cpp
	services/sensorrepository.cpp
	services/abstractsensorrepository.cpp
	services/messageservice.cpp

	data/datacache.cpp
	data/measurementvalidator.cpp
	data/bulkmeasurementvalidator.cpp

	consumers/measurementconsumer.cpp
	consumers/abstractconsumer.cpp

	mqtt/mqttcallback.cpp
	mqtt/mqttclient.cpp
	mqtt/mqttinternalcallback.cpp
)

SET(APP_HEADERS
	include/sensateiot/application.h
	include/sensateiot/getopt.h
)

set(LIB_HEADERS
	auth.h

	config/config.h
	config/database.h
	config/logging.h
	config/mqtt.h
	config/mqttbroker.h

	sensateiot/compiler/compiler-msvc.h
	sensateiot/compiler/compiler.h

	sensateiot/data/datacache.h
	sensateiot/data/bulkmeasurementvalidator.h
	sensateiot/data/measurementvalidator.h

	sensateiot/detail/redblacktree.h
	sensateiot/detail/redblacktree.hpp
	sensateiot/detail/redblacktreeiterator.h
	sensateiot/detail/redblacktreeiterator.hpp
	sensateiot/detail/redblacktreenode.h
	sensateiot/detail/redblacktreenode.hpp

	sensateiot/httpd/httpserver.h
	sensateiot/httpd/httplistener.h
	sensateiot/httpd/httpsession.h
	sensateiot/httpd/httpresponse.h
	sensateiot/httpd/httprequest.h

	sensateiot/http/statushandler.h
	sensateiot/http/measurementhandler.h
	sensateiot/http/abstracthandler.h

	sensateiot/models/apikey.h
	sensateiot/models/objectid.h
	sensateiot/models/rawmeasurement.h
	sensateiot/models/sensor.h
	sensateiot/models/user.h
	sensateiot/mqtt/imqttclient.h

	sensateiot/consumers/abstractconsumer.h
	sensateiot/consumers/measurementconsumer.h

	sensateiot/mqtt/mqttcallback.h
	sensateiot/mqtt/mqttclient.h
	sensateiot/mqtt/mqttinternalcallback.h

	sensateiot/services/abstractapikeyrepository.h
	sensateiot/services/abstractsensorrepository.h
	sensateiot/services/abstractuserrepository.h
	sensateiot/services/apikeyrepository.h
	sensateiot/services/sensorrepository.h
	sensateiot/services/messageservice.h
	sensateiot/services/userrepository.h

	sensateiot/stl/clist.h
	sensateiot/stl/map.h
	sensateiot/stl/referencewrapper.h
	sensateiot/stl/smallvector.h

	sensateiot/util/log.h
	sensateiot/util/mongodbclient.h
	sensateiot/util/mongodbclientpool.h
	sensateiot/util/sha256.h
	sensateiot/util/protobuf.h
	sensateiot/util/base64.h
	sensateiot/util/time.h

	sensateiot.h
)

SET(PROTO_FILES
	${PROJECT_SOURCE_DIR}/libsensateiot/proto/datapoint.proto
)

SET(PROTO_OUTPUT
	${CMAKE_BINARY_DIR}/libsensateiot/proto/datapoint.pb.cc
	${CMAKE_BINARY_DIR}/libsensateiot/proto/datapoint.pb.h
)
add_custom_command(
	OUTPUT ${PROTO_OUTPUT}
	COMMAND ${CONAN_BIN_DIRS_PROTOBUF}/protoc --cpp_out=${CMAKE_BINARY_DIR}/libsensateiot
			-I ${PROJECT_SOURCE_DIR}/libsensateiot ${PROTO_FILES}
	WORKING_DIRECTORY ${CMAKE_BINARY_DIR}/libsensateiot
	DEPENDS ${PROTO_FILES}
	COMMENT "Generating protobuf files"
	VERBATIM
)

FUNCTION( prepend_path SOURCE_FILES INC_PATH )
	FOREACH( SOURCE_FILE ${${SOURCE_FILES}} )
		SET( MODIFIED ${MODIFIED} ${INC_PATH}/${SOURCE_FILE} )
	ENDFOREACH()
	SET( ${SOURCE_FILES} ${MODIFIED} PARENT_SCOPE )
ENDFUNCTION()

SET(GENERIC_HEADERS 
	${LIB_HEADERS}
)

prepend_path(GENERIC_HEADERS "libsensateiot/include" )

SET(GENERIC_HEADERS 
	${GENERIC_HEADERS}
	${APP_HEADERS}
)

SET(INCLUDE_DIR ${PROJECT_SOURCE_DIR})
prepend_path(GENERIC_HEADERS ${INCLUDE_DIR})

#add_custom_target(proto-dep DEPENDS ${PROTO_SRCS} ${PROTO_HDRS})

add_library(sensateiot SHARED ${SOURCES} ${GENERIC_HEADERS} ${PROTO_OUTPUT})
#add_dependencies(sensateiot proto-dep)
target_link_libraries(sensateiot ${CONAN_LIBS})
target_include_directories(sensateiot PRIVATE ${PROJECT_BINARY_DIR}/libsensateiot)

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

SET(${SOURCES} ${SOURCES})
SET(${LIB_HEADERS} ${LIB_HEADERS})

message(STATUS "Generating MSVS filters.")
foreach(source IN LISTS ${SOURCES})
    get_filename_component(source_path "${source}" PATH)
    string(REPLACE "/" "\\" source_path_msvc "Source Files\\${source_path}")
    source_group("${source_path_msvc}" FILES "${source}")
endforeach()

foreach(source IN LISTS ${LIB_HEADERS})
    get_filename_component(source_path "${source}" PATH)
	SET(source "include/${source}")
    string(REPLACE "/" "\\" header_path_msvc "Header Files\\${source_path}")
    source_group("${header_path_msvc}" FILES "${source}")
endforeach()
