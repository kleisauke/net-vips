FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine3.10 AS build
LABEL maintainer "Kleis Auke Wolthuizen <info@kleisauke.nl>"

WORKDIR /app

ENTRYPOINT ["./build.sh"]
