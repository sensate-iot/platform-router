#
# Docker file for MQTT handler.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM debian:stretch

ARG source


RUN apt-get update && \
	apt-get -y install bash openssl curl

COPY . /app
WORKDIR /app

ENTRYPOINT ["MqttHandler"]
