FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine3.12 AS build
LABEL maintainer="Kleis Auke Wolthuizen <info@kleisauke.nl>"

# Increase the minimum stack size to 2MB
ENV VIPS_MIN_STACK_SIZE=2m

RUN apk add bash ttf-dejavu --update-cache

WORKDIR /app

ENTRYPOINT ["./build.sh"]
