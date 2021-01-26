#
# Docker file for MQTT handler.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env

WORKDIR /build
COPY . .
RUN dotnet restore -r linux-x64 SensateIoT.Platform.Ingress.MqttService/SensateIoT.Platform.Ingress.MqttService.csproj
RUN dotnet publish -c Release -o /build/binaries -r linux-x64 --no-restore --no-restore SensateIoT.Platform.Ingress.MqttService/SensateIoT.Platform.Ingress.MqttService.csproj

FROM mcr.microsoft.com/dotnet/runtime:5.0

COPY --from=build-env /build/binaries /app
WORKDIR /app
ENTRYPOINT ["/app/SensateIoT.Platform.Ingress.MqttService"]
