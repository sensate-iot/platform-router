#
# Docker file for NetworkApi.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

WORKDIR /build
COPY Core/*.csproj Core/
COPY ApiCore/*.csproj ApiCore/
COPY NetworkApi/*.csproj NetworkApi/
COPY COPYING .
RUN dotnet restore -r linux-musl-x64 NetworkApi/NetworkApi.csproj

COPY Core/ Core/
COPY NetworkApi/ NetworkApi/
COPY ApiCore/ ApiCore/
RUN dotnet publish -c Release -o /app/bin -r linux-musl-x64 --no-restore --self-contained true --no-restore NetworkApi/NetworkApi.csproj

FROM alpine:3.11

COPY --from=build-env /app /app
COPY NetworkApi/Settings/hosting.json /app/hosting.json
WORKDIR /app
RUN apk add --no-cache libstdc++ libintl icu

EXPOSE 5000
ENTRYPOINT ["bin/NetworkApi"]
