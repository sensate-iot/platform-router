#
# Docker file for WebSocketHandler.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

WORKDIR /build
COPY . .
RUN dotnet restore -r linux-musl-x64 Ingress/SensateService.Ingress.WebSocketHandler/SensateService.Ingress.WebSocketHandler.csproj
RUN dotnet publish -c Release -o /app/bin -r linux-musl-x64 --no-restore --self-contained true --no-restore Ingress/SensateService.Ingress.WebSocketHandler/SensateService.Ingress.WebSocketHandler.csproj

FROM alpine:3.11

COPY --from=build-env /app /app
COPY Ingress/SensateService.Ingress.WebSocketHandler/SensateService.Ingress.WebSocketHandler/Settings/hosting.json /app/hosting.json
WORKDIR /app
RUN apk add --no-cache libstdc++ libintl icu

EXPOSE 5000 
ENTRYPOINT ["bin/SensateService.Ingress.WebSocketHandler"]
