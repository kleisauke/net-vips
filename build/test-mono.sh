#!/usr/bin/env bash

set -eo pipefail

###########################################################################
# CONFIGURATION
###########################################################################

BUILD_DIR=$(cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd)
SCRIPT_DIR=$(dirname "$BUILD_DIR")

XUNIT_RUNNER="$SCRIPT_DIR/packages/xunit.runner.console/tools/net472/xunit.console.exe"
BUILD_PROJECT_FILE="$SCRIPT_DIR/tests/NetVips.Tests/NetVips.Tests.csproj"
DLL_FILE="$SCRIPT_DIR/tests/NetVips.Tests/bin/Release/net472/NetVips.Tests.dll"

# https://github.com/dotnet/runtime/issues/11540
if [ "$(uname)" == "Darwin" ]; then
    export DYLD_LIBRARY_PATH=$(dirname "$DLL_FILE")${DYLD_LIBRARY_PATH:+:$DYLD_LIBRARY_PATH}
elif [ "$(expr substr $(uname -s) 1 5)" == "Linux" ]; then
    export LD_LIBRARY_PATH=$(dirname "$DLL_FILE")${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}
fi

# Parse arguments
while [ $# -gt 0 ]; do
    case $1 in
        --global-vips) USE_NUGET_BINARIES=false ;;
        *) echo "Unknown parameter passed: $1" >&2; exit 1 ;;
    esac
    shift
done

###########################################################################
# EXECUTION
###########################################################################

if ! [ -x "$(command -v mono)" ]; then
    echo 'Error: mono is not installed.' >&2
    exit 1
fi

echo "Mono version $(mono --version)"

nuget install xunit.runner.console -ExcludeVersion -o packages
msbuild $BUILD_PROJECT_FILE -t:build -restore -p:TargetFramework="net472" -p:Configuration=Release -p:TestWithNuGetBinaries=${USE_NUGET_BINARIES:-true} -nodeReuse:false
mono $XUNIT_RUNNER $DLL_FILE
