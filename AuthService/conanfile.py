#
#
#

from conans import ConanFile, CMake


class AuthService(ConanFile):
    settings = "os", "compiler", "build_type", "arch"
    requires = [
        "mongo-cxx-driver/3.3.0@bincrafters/stable",
        ("mongo-c-driver/1.16.1@bincrafters/stable", "override"),
        "paho-mqtt-cpp/1.1",
        "libpqxx/7.0.5",
        ("openssl/1.1.1f", "override")
    ]
    generators = "cmake", "gcc", "txt"
    default_options = {
        "paho-mqtt-cpp:ssl": True,
        "libpq:with_openssl": False
    }

    def build(self):
        cmake = CMake(self)
        cmake.configure()
        cmake.build()
        cmake.install()
