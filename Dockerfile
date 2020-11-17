FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine3.12 AS build
LABEL maintainer="Kleis Auke Wolthuizen <info@kleisauke.nl>"

RUN apk --no-cache add bash ttf-dejavu fontconfig && \
    fc-cache -f

WORKDIR /app

ENTRYPOINT ["./build.sh"]
