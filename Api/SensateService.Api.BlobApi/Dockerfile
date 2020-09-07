#
# Docker file for BlobApi.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

WORKDIR /build

COPY . .
RUN dotnet restore -r linux-musl-x64 Api/SensateService.Api.BlobApi/SensateService.Api.BlobApi.csproj
RUN dotnet publish -c Release -o /app/bin -r linux-musl-x64 --no-restore --self-contained true --no-restore Api/SensateService.Api.BlobApi/SensateService.Api.BlobApi.csproj

FROM alpine:3.11

COPY --from=build-env /app /app
COPY Api/SensateService.Api.BlobApi/SensateService.Api.BlobApi/Settings/hosting.json /app/hosting.json
WORKDIR /app
RUN apk add --no-cache libstdc++ libintl icu

EXPOSE 5000
ENTRYPOINT ["bin/SensateService.Api.BlobApi"]
