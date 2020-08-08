#
# CMakeLists for libauthservice.
#

include (${PROJECT_SOURCE_DIR}/cmake/util.cmake)
include (${PROJECT_SOURCE_DIR}/cmake/flags.cmake)

# Lib sensateiot source / header files

SET(SOURCES
	models/apikey.cpp
	models/user.cpp
	models/sensor.cpp
	models/measurement.cpp
	models/message.cpp

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
	util/gzip.cpp
	util/base64.cpp

	httpd/httpserver.cpp
	httpd/httpsession.cpp
	httpd/httplistener.cpp
	httpd/httpresponse.cpp
	httpd/httprequest.cpp

	http/statushandler.cpp
	http/bulkmeasurementhandler.cpp
	http/measurementhandler.cpp
	http/messagehandler.cpp
	http/bulkmessagehandler.cpp
	http/abstracthandler.cpp

	services/userrepository.cpp
	services/apikeyrepository.cpp
	services/sensorrepository.cpp
	services/abstractsensorrepository.cpp
	services/messageservice.cpp
	services/abstractpostgresqlrepository.cpp

	data/datacache.cpp
	data/measurementvalidator.cpp
	data/messagevalidator.cpp
	data/bulkmessagevalidator.cpp
	data/bulkmeasurementvalidator.cpp

	consumers/measurementconsumer.cpp
	consumers/messageconsumer.cpp
	consumers/commandconsumer.cpp

	mqtt/basemqttclient.cpp
	mqtt/internalmqttclient.cpp
	mqtt/mqttinternalcallback.cpp

	commands/command.cpp
	commands/flushusercommandhandler.cpp
	commands/flushsensorcommandhandler.cpp
	commands/flushkeycommandhandler.cpp
)

set(LIB_HEADERS
	auth.h

	config/config.h
	config/database.h
	config/logging.h
	config/mqtt.h
	config/mqttbroker.h

	sensateiot/version.h

	sensateiot/compiler/compiler-msvc.h
	sensateiot/compiler/compiler.h

	sensateiot/data/datacache.h
	sensateiot/data/bulkmeasurementvalidator.h
	sensateiot/data/measurementvalidator.h
	sensateiot/data/messagevalidator.h
	sensateiot/data/bulkmessagevalidator.h

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
	sensateiot/http/messagehandler.h
	sensateiot/http/bulkmessagehandler.h
	sensateiot/http/measurementhandler.h
	sensateiot/http/bulkmeasurementhandler.h
	sensateiot/http/abstracthandler.h

	sensateiot/models/apikey.h
	sensateiot/models/objectid.h
	sensateiot/models/measurement.h
	sensateiot/models/sensor.h
	sensateiot/models/user.h
	sensateiot/models/message.h

	sensateiot/consumers/abstractconsumer.h
	sensateiot/consumers/measurementconsumer.h
	sensateiot/consumers/messageconsumer.h
	sensateiot/consumers/commandconsumer.h

	sensateiot/mqtt/imqttclient.h
	sensateiot/mqtt/basemqttclient.h
	sensateiot/mqtt/mqttinternalcallback.h
	sensateiot/mqtt/internalmqttclient.h

	sensateiot/services/abstractapikeyrepository.h
	sensateiot/services/abstractsensorrepository.h
	sensateiot/services/abstractuserrepository.h
	sensateiot/services/apikeyrepository.h
	sensateiot/services/sensorrepository.h
	sensateiot/services/messageservice.h
	sensateiot/services/userrepository.h
	sensateiot/services/abstractpostgresqlrepository.h

	sensateiot/stl/clist.h
	sensateiot/stl/map.h
	sensateiot/stl/dictionary.h
	sensateiot/stl/referencewrapper.h
	sensateiot/stl/smallvector.h

	sensateiot/util/log.h
	sensateiot/util/mongodbclient.h
	sensateiot/util/mongodbclientpool.h
	sensateiot/util/sha256.h
	sensateiot/util/protobuf.h
	sensateiot/util/gzip.h
	sensateiot/util/base64.h
	sensateiot/util/time.h

	sensateiot/commands/abstractcommandhandler.h
	sensateiot/commands/command.h
	sensateiot/commands/flushusercommandhandler.h
	sensateiot/commands/flushsensorcommandhandler.h
	sensateiot/commands/flushkeycommandhandler.h

	sensateiot.h
)

SET(PROTO_FILES
	${PROJECT_SOURCE_DIR}/libsensateiot/proto/measurement.proto
	${PROJECT_SOURCE_DIR}/libsensateiot/proto/message.proto
)

SET(PROTO_OUTPUT
	${CMAKE_BINARY_DIR}/libsensateiot/proto/measurement.pb.cc
	${CMAKE_BINARY_DIR}/libsensateiot/proto/measurement.pb.h
	${CMAKE_BINARY_DIR}/libsensateiot/proto/message.pb.cc
	${CMAKE_BINARY_DIR}/libsensateiot/proto/message.pb.h
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

SET(GENERIC_HEADERS  ${LIB_HEADERS})
SET(${SOURCES}       ${SOURCES})
SET(${LIB_HEADERS}   ${LIB_HEADERS})
SET(INCLUDE_DIR ${PROJECT_SOURCE_DIR})

prepend_path(GENERIC_HEADERS "libsensateiot/include" )
prepend_path(GENERIC_HEADERS ${INCLUDE_DIR})

add_library(sensateiot SHARED ${SOURCES} ${GENERIC_HEADERS} ${PROTO_OUTPUT})
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
