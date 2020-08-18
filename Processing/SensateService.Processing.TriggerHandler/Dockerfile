#
# Docker file for TriggerHandler.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

WORKDIR /build
COPY Core/*.csproj Core/
COPY TriggerHandler/*.csproj TriggerHandler/
COPY COPYING .
RUN dotnet restore -r linux-musl-x64 TriggerHandler/TriggerHandler.csproj

COPY Core/ Core/
COPY TriggerHandler/ TriggerHandler/
RUN dotnet publish -c Release -o /app/bin -r linux-musl-x64 --no-restore --self-contained true --no-restore TriggerHandler/TriggerHandler.csproj

FROM alpine:3.11

COPY --from=build-env /app /app
WORKDIR /app
RUN apk add --no-cache libstdc++ libintl icu

ENTRYPOINT ["bin/TriggerHandler"]
