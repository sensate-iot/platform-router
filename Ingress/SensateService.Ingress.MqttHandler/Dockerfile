#
# Docker file for MQTT handler.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

WORKDIR /build
COPY . .
RUN dotnet restore -r linux-musl-x64 Ingress/SensateService.Ingress.MqttHandler/SensateService.Ingress.MqttHandler.csproj
RUN dotnet publish -c Release -o /app/bin -r linux-musl-x64 --no-restore --self-contained true --no-restore Ingress/SensateService.Ingress.MqttHandler/SensateService.Ingress.MqttHandler.csproj

FROM alpine:3.11

COPY --from=build-env /app /app
WORKDIR /app
RUN apk add --no-cache libstdc++ libintl icu

ENTRYPOINT ["bin/SensateService.Ingress.MqttHandler"]
