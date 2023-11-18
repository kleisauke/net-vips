FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
LABEL maintainer="Kleis Auke Wolthuizen <info@kleisauke.nl>"

RUN apk add bash font-noto --update-cache

WORKDIR /app

CMD ["./build.sh"]
