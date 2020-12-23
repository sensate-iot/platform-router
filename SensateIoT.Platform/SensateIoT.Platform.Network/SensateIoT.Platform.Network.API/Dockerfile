#
# Docker file for the storage service.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env

WORKDIR /build

COPY . .
RUN dotnet restore -r linux-musl-x64 SensateIoT.Platform.Network.API/SensateIoT.Platform.Network.API.csproj
RUN dotnet publish -c Release -o /app/bin -r linux-musl-x64 --no-restore --self-contained true --no-restore SensateIoT.Platform.Network.API/SensateIoT.Platform.Network.API.csproj

FROM alpine:3.11

COPY --from=build-env /app /app
WORKDIR /app
RUN apk add --no-cache libstdc++ libintl icu

ENTRYPOINT ["bin/SensateIoT.Platform.Network.API"]
