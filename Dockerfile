FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine3.9 AS build
LABEL maintainer "Kleis Auke Wolthuizen <info@kleisauke.nl>"

WORKDIR /app

ENTRYPOINT ["./build.sh"]
