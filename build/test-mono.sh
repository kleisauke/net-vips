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

if [ "$(uname)" == "Darwin" ]; then
    RID="osx-x64"

    # Prefer Homebrew's glib to avoid compatibility issues
    # with the one provided by Mono.
    export DYLD_LIBRARY_PATH=$(brew --prefix glib)/lib
elif [ "$(expr substr $(uname -s) 1 5)" == "Linux" ]; then
    RID="linux-x64"
fi

###########################################################################
# EXECUTION
###########################################################################

if ! [ -x "$(command -v mono)" ]; then
    echo 'Error: mono is not installed.' >&2
    exit 1
fi

echo "Mono version $(mono --version)"

nuget install xunit.runner.console -ExcludeVersion -o packages
msbuild $BUILD_PROJECT_FILE -t:build -restore /p:Configuration=Release ${RID:+/p:RuntimeIdentifier=$RID} /p:TestWithNuGetBinaries=false /nodeReuse:false
mono $XUNIT_RUNNER $DLL_FILE
