#
# Docker file for MQTT handler.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

WORKDIR /build
COPY Core/*.csproj Core/
COPY StorageClient/*.csproj StorageClient/
COPY COPYING .
RUN dotnet restore -r linux-musl-x64 StorageClient/StorageClient.csproj

COPY Core/ Core/
COPY StorageClient/ StorageClient/
RUN dotnet publish -c Release -o /app/bin -r linux-musl-x64 --no-restore --self-contained true --no-restore StorageClient/StorageClient.csproj

FROM alpine:3.11

COPY --from=build-env /app /app
WORKDIR /app
RUN apk add --no-cache libstdc++ libintl icu

ENTRYPOINT ["bin/StorageClient"]
