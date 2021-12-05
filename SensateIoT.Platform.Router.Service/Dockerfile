#
# Docker file for the routing service.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

WORKDIR /build

COPY . .
RUN dotnet restore -r linux-x64 SensateIoT.Platform.Router.Service/SensateIoT.Platform.Router.Service.csproj
RUN dotnet publish -c Release -o /build/binaries -r linux-x64 --no-restore SensateIoT.Platform.Router.Service/SensateIoT.Platform.Router.Service.csproj

FROM mcr.microsoft.com/dotnet/aspnet:6.0

COPY --from=build-env /build/binaries /app
COPY SensateIoT.Platform.Router.Service/appsettings.json /app/appsettings.json

EXPOSE 80
EXPOSE 8080
WORKDIR /app
ENTRYPOINT ["/app/SensateIoT.Platform.Router.Service", "--urls", "http://*:80"]
