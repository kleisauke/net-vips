#!/usr/bin/env bash

set -eo pipefail

###########################################################################
# CONFIGURATION
###########################################################################

SCRIPT_DIR=$(cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd)
BUILD_PROJECT_FILE="$SCRIPT_DIR/build/NetVips.Build.csproj"

# Default arguments
TARGET_FRAMEWORK="net5.0"

# Parse arguments
POSITIONAL=()

while [ $# -gt 0 ]; do
    case $1 in
        -f|--framework) TARGET_FRAMEWORK="$2"; shift ;;
        *) POSITIONAL+=("$1") ;;
    esac
    shift
done

# Restore positional parameters
set -- "${POSITIONAL[@]}"

###########################################################################
# EXECUTION
###########################################################################

if ! [ -x "$(command -v dotnet)" ]; then
    echo 'Error: dotnet is not installed.' >&2
    exit 1
fi

echo "Microsoft (R) .NET Core SDK version $(dotnet --version)"

dotnet build "$BUILD_PROJECT_FILE" -p:TargetFramework="$TARGET_FRAMEWORK" -nodeReuse:false 
dotnet run --project "$BUILD_PROJECT_FILE" --no-build -- "$@"
