#!/usr/bin/env bash

set -eo pipefail
SCRIPT_DIR=$(cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd)

###########################################################################
# CONFIGURATION
###########################################################################

BUILD_PROJECT_FILE="$SCRIPT_DIR/build/NetVips.Build.csproj"

export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_MULTILEVEL_LOOKUP=0
export NUKE_TELEMETRY_OPTOUT=1

###########################################################################
# EXECUTION
###########################################################################

# Do not use -x to check for the presence of dotnet,
# because that might fail on bash under Alpine 3.14, see:
# https://github.com/alpinelinux/docker-alpine/issues/156
# https://wiki.alpinelinux.org/wiki/Release_Notes_for_Alpine_3.14.0#faccessat2
if ! [ -e "$(command -v dotnet)" ]; then
    echo 'Error: dotnet is not installed.' >&2
    exit 1
fi

echo "Microsoft (R) .NET Core SDK version $(dotnet --version)"

dotnet build "$BUILD_PROJECT_FILE" -nodeReuse:false -p:UseSharedCompilation=false -nologo -clp:NoSummary --verbosity quiet
dotnet run --project "$BUILD_PROJECT_FILE" --no-build -- "$@"
