#!/usr/bin/env bash

set -e

if [[ $TRAVIS_OS_NAME == 'linux' ]]; then
    uname -a

    . $TRAVIS_BUILD_DIR/build/install-vips.sh \
      --without-python
fi