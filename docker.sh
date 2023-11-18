#!/usr/bin/env bash

if ! [ -x "$(command -v docker)" ]; then
  echo "Please install docker"
  exit 1
fi

# Ensure latest .NET SDK
docker pull mcr.microsoft.com/dotnet/sdk:8.0-alpine

# Create a machine image with all the required build tools pre-installed
docker build . -t netvips

# Run build scripts inside container, with netvips directory
# mounted at /app
docker run --rm -t \
    -v $PWD:/app \
    netvips
