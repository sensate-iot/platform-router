#
# Docker file for DataApi.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM python:3-buster AS build-env

WORKDIR /build
COPY . .
RUN pip install conan && \
 apt-get update && \
 apt-get install -y cmake build-essential && \
 mkdir compile-out && cd compile-out/ && \
 conan install --build missing .. && \
 cmake -DCMAKE_BUILD_TYPE=Release -DCONFIG_BUILD_TESTS=False -DCMAKE_INSTALL_PREFIX=`pwd`/install ../ && \
 make -j4 && make install
	
FROM debian:buster-slim

ENV LD_LIBRARY_PATH=/app/lib:${LD_LIBRARY_PATH}
COPY --from=build-env /build/compile-out/install /app
WORKDIR /app

EXPOSE 8080
ENTRYPOINT ["bin/auth-service", "-c", "/app/etc/appsettings.json"]
