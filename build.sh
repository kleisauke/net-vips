#!/bin/sh

set -e

###########################################################################
# CONFIGURATION
###########################################################################

SCRIPT_DIR=$(pwd)
BUILD_PROJECT_FILE="$SCRIPT_DIR/build/NetVips.Build.csproj"

###########################################################################
# EXECUTION
###########################################################################

if ! [ -x "$(command -v dotnet)" ]; then
    echo 'Error: dotnet is not installed.' >&2
    exit 1
fi

echo "Microsoft (R) .NET Core SDK version $(dotnet --version)"

dotnet build "$BUILD_PROJECT_FILE" /nodeReuse:false
dotnet run --project "$BUILD_PROJECT_FILE" --no-build -- "$@"
