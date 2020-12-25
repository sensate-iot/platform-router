#
# Docker file for the trigger service.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env

WORKDIR /build

COPY . .
RUN dotnet restore -r linux-musl-x64 SensateIoT.Platform.Network.Router/SensateIoT.Platform.Network.Router.csproj
RUN dotnet publish -c Release -o /app/bin -r linux-musl-x64 --no-restore --self-contained true --no-restore SensateIoT.Platform.Network.Router/SensateIoT.Platform.Network.Router.csproj

FROM alpine:3.12

COPY --from=build-env /app /app
COPY SensateIoT.Platform.Network.Router/appsettings.json /app/appsettings.json

WORKDIR /app
RUN apk add --no-cache libstdc++ libintl icu

ENTRYPOINT ["bin/SensateIoT.Platform.Network.Router"]
