#!/usr/bin/env bash

if ! type docker > /dev/null; then
  echo "Please install docker"
  exit 1
fi

# Ensure latest .NET Core SDK
docker pull mcr.microsoft.com/dotnet/core/sdk:2.2-stretch

# Create a machine image with all the required build tools pre-installed
docker build . -t netvips

# Run build scripts inside container, with netvips directory 
# mounted at /app
docker run --rm -t \
	-v $PWD:/app \
	netvips
