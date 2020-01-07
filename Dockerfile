FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine3.10 AS build
LABEL maintainer "Kleis Auke Wolthuizen <info@kleisauke.nl>"

ENV FONTCONFIG_PATH=/etc/fonts

RUN apk --no-cache add ttf-dejavu fontconfig && \
    fc-cache -f

WORKDIR /app

ENTRYPOINT ["./build.sh"]
