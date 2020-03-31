#
# Docker file for WebSocketHandler.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

WORKDIR /build
COPY Core/*.csproj Core/
COPY ApiCore/*.csproj ApiCore/
COPY WebSocketHandler/*.csproj WebSocketHandler/
COPY COPYING .
RUN dotnet restore -r linux-musl-x64 WebSocketHandler/WebSocketHandler.csproj

COPY Core/ Core/
COPY WebSocketHandler/ WebSocketHandler/
COPY ApiCore/ ApiCore/
RUN dotnet publish -c Release -o /app/bin -r linux-musl-x64 --no-restore --self-contained true --no-restore WebSocketHandler/WebSocketHandler.csproj

FROM alpine:3.11

COPY --from=build-env /app /app
COPY WebSocketHandler/Settings/hosting.json /app/hosting.json
WORKDIR /app
RUN apk add --no-cache libstdc++ libintl icu

EXPOSE 5000 
ENTRYPOINT ["bin/WebSocketHandler"]
