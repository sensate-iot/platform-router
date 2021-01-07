#
# Docker file for the network API.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env

WORKDIR /build

COPY . .
RUN dotnet restore -r linux-x64 SensateIoT.Platform.Network.API/SensateIoT.Platform.Network.API.csproj
RUN dotnet publish -c Release -o /build/binaries -r linux-x64 --no-restore SensateIoT.Platform.Network.API/SensateIoT.Platform.Network.API.csproj

FROM mcr.microsoft.com/dotnet/aspnet:5.0

COPY --from=build-env /build/binaries /app
COPY SensateIoT.Platform.Network.API/appsettings.json /app/appsettings.json

EXPOSE 80
EXPOSE 8080
WORKDIR /app
ENTRYPOINT ["/app/SensateIoT.Platform.Network.API", "--urls", "http://*:80"]
