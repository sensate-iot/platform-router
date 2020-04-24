#
#
#

from conans import ConanFile, CMake
import os


class AuthService(ConanFile):
    settings = "os", "compiler", "build_type", "arch"
    generators = "cmake"

    def is_windows(self):
        return os.name == 'nt'

    def requirements(self):
        if self.is_windows():
            self.default_options = {
                "libpq:with_openssl": False
            }

            self.requires("mongo-cxx-driver/3.3.0@bincrafters/stable")
            self.requires("mongo-c-driver/1.16.1@bincrafters/stable", override=True)
            self.requires("paho-mqtt-cpp/1.1", override=False)
            self.requires("libpqxx/7.0.5", override=False)
            self.requires("libpq/11.5", override=True)
            self.requires("boost_log/1.67.0@bincrafters/stable")
            self.requires("zlib/1.2.11", override=True)
        else:
            self.default_options = {
                "libpq:with_openssl": False,
                "paho-mqtt-cpp:ssl": True,
                "libpq:shared": False,
                "libpqxx:shared": False
            }

            self.requires("mongo-cxx-driver/3.3.0@bincrafters/stable")
            self.requires("mongo-c-driver/1.16.1@bincrafters/stable", override=True)
            self.requires("boost_log/1.67.0@bincrafters/stable")
            self.requires("zlib/1.2.11", override=True)
            self.requires("paho-mqtt-cpp/1.1", override=False)
            self.requires("libpq/11.5", override=True)
            self.requires("libpqxx/7.0.5", override=False)
            self.requires("openssl/1.1.1f", override=False)

    def imports(self):
        self.copy('libpq.so*', '', 'lib')

    def build(self):
        cmake = CMake(self)
        cmake.configure()
        cmake.build()
        cmake.install()
