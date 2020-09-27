#
# Conan project configuration.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

from conans import ConanFile, CMake
import os


def is_windows():
    return os.name == 'nt'


class AuthService(ConanFile):
    settings = "os", "compiler", "build_type", "arch"
    generators = "cmake"
    default_options = {
        "libpq:with_openssl": False,
        "paho-mqtt-cpp:ssl": True,
        "paho-mqtt-c:ssl": True,
        "libpq:shared": False,
        "libpqxx:shared": False
    }

    def configure(self):
        if is_windows():
            return

        self.options["mongo-c-driver"].shared = True

    def requirements(self):
        self.requires("mongo-c-driver/1.16.1@bincrafters/stable")
        self.requires("boost/1.73.0")
        self.requires("zlib/1.2.11", override=True)
        self.requires("libpqxx/7.0.5", override=False)
        self.requires("re2/20200601")
        self.requires("protobuf/3.11.4")
        self.requires("rapidjson/1.1.0")
        self.requires("catch2/2.11.1")
        self.requires("paho-mqtt-cpp/1.1", override=False)

        if is_windows():
            self.requires("paho-mqtt-c/1.3.4", override=True)
        else:
            self.requires("paho-mqtt-c/1.3.1", override=True)
            self.requires("openssl/1.1.1g", override=True)

    def imports(self):
        self.copy('libpq.so*', 'libdist', 'lib')
        self.copy('lib*mongo*.so*', 'libdist', 'lib')
        self.copy('lib*bson*.so*', 'libdist', 'lib')

    def build(self):
        cmake = CMake(self)
        cmake.configure()
        cmake.build()
        cmake.install()
