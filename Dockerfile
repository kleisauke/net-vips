FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine3.12 AS build
LABEL maintainer="Kleis Auke Wolthuizen <info@kleisauke.nl>"

ENV FONTCONFIG_PATH=/etc/fonts \
    # https://github.com/dotnet/runtime/issues/28303#issuecomment-646994218
    DOTNET_RUNTIME_ID=linux-musl-x64

RUN apk --no-cache add bash ttf-dejavu fontconfig && \
    fc-cache -f

WORKDIR /app

ENTRYPOINT ["./build.sh"]
