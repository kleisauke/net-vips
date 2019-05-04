# FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine3.9 AS build
FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
LABEL maintainer "Kleis Auke Wolthuizen <info@kleisauke.nl>"

WORKDIR /app

ENTRYPOINT ["./build.sh"]
