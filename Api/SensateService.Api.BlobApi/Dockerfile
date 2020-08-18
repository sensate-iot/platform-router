#
# Docker file for BlobApi.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

WORKDIR /build
COPY Core/*.csproj Core/
COPY ApiCore/*.csproj ApiCore/
COPY BlobApi/*.csproj BlobApi/
COPY COPYING .
RUN dotnet restore -r linux-musl-x64 BlobApi/BlobApi.csproj

COPY Core/ Core/
COPY BlobApi/ BlobApi/
COPY ApiCore/ ApiCore/
RUN dotnet publish -c Release -o /app/bin -r linux-musl-x64 --no-restore --self-contained true --no-restore BlobApi/BlobApi.csproj

FROM alpine:3.11

COPY --from=build-env /app /app
COPY BlobApi/Settings/hosting.json /app/hosting.json
WORKDIR /app
RUN apk add --no-cache libstdc++ libintl icu

EXPOSE 5000
ENTRYPOINT ["bin/BlobApi"]
