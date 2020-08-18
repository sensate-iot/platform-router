#
# Docker file for DashboardApi.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

WORKDIR /build
COPY Core/*.csproj Core/
COPY ApiCore/*.csproj ApiCore/
COPY DashboardApi/*.csproj DashboardApi/
COPY COPYING .
RUN dotnet restore -r linux-musl-x64 DashboardApi/DashboardApi.csproj

COPY Core/ Core/
COPY DashboardApi/ DashboardApi/
COPY ApiCore/ ApiCore/
RUN dotnet publish -c Release -o /app/bin -r linux-musl-x64 --no-restore --self-contained true --no-restore DashboardApi/DashboardApi.csproj

FROM alpine:3.11

COPY --from=build-env /app /app
COPY DashboardApi/Settings/hosting.json /app/hosting.json
WORKDIR /app
RUN apk add --no-cache libstdc++ libintl icu

EXPOSE 5000
ENTRYPOINT ["bin/DashboardApi"]
