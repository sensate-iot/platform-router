#
# Docker file for AuthApi.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

WORKDIR /build

COPY . .
RUN dotnet restore -r linux-musl-x64 Api/SensateService.Api.AuthApi/SensateService.Api.AuthApi.csproj
RUN dotnet publish -c Release -o /app/bin -r linux-musl-x64 --no-restore --self-contained true --no-restore Api/SensateService.Api.AuthApi/SensateService.Api.AuthApi.csproj

FROM alpine:3.11

COPY --from=build-env /app /app
COPY Api/SensateService.Api.AuthApi/SensateService.Api.AuthApi/Settings/hosting.json /app/hosting.json
COPY Api/SensateService.Api.AuthApi/SensateService.Api.AuthApi/Templates /app/Templates
WORKDIR /app
RUN apk add --no-cache libstdc++ libintl icu

EXPOSE 5000
ENTRYPOINT ["bin/SensateService.Api.AuthApi"]
