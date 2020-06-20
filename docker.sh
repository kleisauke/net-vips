#!/usr/bin/env bash

if ! [ -x "$(command -v docker)" ]; then
  echo "Please install docker"
  exit 1
fi

# Ensure latest .NET Core SDK
docker pull mcr.microsoft.com/dotnet/core/sdk:3.1-alpine3.12

# Create a machine image with all the required build tools pre-installed
docker build . -t netvips

# Run build scripts inside container, with netvips directory 
# mounted at /app
docker run --rm -t \
    -v $PWD:/app \
    netvips
