#
# Docker file for MQTT handler.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

WORKDIR /build
COPY Core/*.csproj Core/
COPY MqttHandler/*.csproj MqttHandler/
COPY COPYING .
RUN dotnet restore -r linux-x64 MqttHandler/MqttHandler.csproj

COPY Core/ Core/
COPY MqttHandler/ MqttHandler/
RUN dotnet publish -c Release -o /app/bin -r linux-x64 --no-restore --self-contained true --no-restore MqttHandler/MqttHandler.csproj

FROM debian:buster-slim

WORKDIR /app
COPY --from=build-env /app /app

ENTRYPOINT ["/app/bin/MqttHandler"]
