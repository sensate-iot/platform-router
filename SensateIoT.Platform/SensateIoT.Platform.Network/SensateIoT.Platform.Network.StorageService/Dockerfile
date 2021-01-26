#
# Docker file for the storage service.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env

WORKDIR /build

COPY . .
RUN dotnet restore -r linux-x64 SensateIoT.Platform.Network.StorageService/SensateIoT.Platform.Network.StorageService.csproj
RUN dotnet publish -c Release -o /build/binaries -r linux-x64 --no-restore SensateIoT.Platform.Network.StorageService/SensateIoT.Platform.Network.StorageService.csproj

FROM mcr.microsoft.com/dotnet/runtime:5.0

COPY --from=build-env /build/binaries /app
COPY SensateIoT.Platform.Network.StorageService/appsettings.json /app/appsettings.json

EXPOSE 8080
WORKDIR /app
ENTRYPOINT ["/app/SensateIoT.Platform.Network.StorageService"]
