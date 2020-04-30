#
#
#

from conans import ConanFile, CMake
import os


class AuthService(ConanFile):
    settings = "os", "compiler", "build_type", "arch"
    generators = "cmake"
    default_options = {
        "libpq:with_openssl": False,
        "paho-mqtt-cpp:ssl": True,
        "paho-mqtt-c:ssl": True,
        "libpq:shared": False,
        "libpqxx:shared": False,
        "mongo-cxx-driver:shared": True
    }

    def configure(self):
        if not self.is_windows():
            return

        self.options['mongo-cxx-driver'].shared = False

    def is_windows(self):
        return os.name == 'nt'

    def requirements(self):
        if self.is_windows():
            self.requires("mongo-cxx-driver/3.3.0@bincrafters/stable")
            self.requires("mongo-c-driver/1.16.1@bincrafters/stable", override=True)
            self.requires("paho-mqtt-cpp/1.1", override=False)
            self.requires("libpqxx/7.0.5", override=False)
            self.requires("libpq/11.5", override=True)
            self.requires("boost_log/1.67.0@bincrafters/stable")
            self.requires("zlib/1.2.11", override=True)
        else:
            self.requires("mongo-cxx-driver/3.3.0@bincrafters/stable")
            self.requires("mongo-c-driver/1.16.1@bincrafters/stable", override=True)
            self.requires("boost_log/1.67.0@bincrafters/stable")
            self.requires("zlib/1.2.11", override=True)
            self.requires("paho-mqtt-cpp/1.1", override=False)
            self.requires("paho-mqtt-c/1.3.1", override=True)
            self.requires("libpq/11.5", override=True)
            self.requires("libpqxx/7.0.5", override=False)
            self.requires("openssl/1.1.1f", override=True)

    def imports(self):
        self.copy('libpq.so*', 'libdist', 'lib')
        self.copy('lib*mongo*.so*', 'libdist', 'lib')
        self.copy('lib*bson*.so*', 'libdist', 'lib')

    def build(self):
        cmake = CMake(self)
        cmake.configure()
        cmake.build()
        cmake.install()
