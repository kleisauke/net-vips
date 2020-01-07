#!/usr/bin/env bash

set -e

if [[ $TRAVIS_OS_NAME == 'linux' ]]; then
    uname -a

    # add support for WebP
    wget http://archive.ubuntu.com/ubuntu/pool/main/libw/libwebp/libwebp-dev_0.6.1-2_amd64.deb
    wget http://archive.ubuntu.com/ubuntu/pool/main/libw/libwebp/libwebpdemux2_0.6.1-2_amd64.deb
    wget http://archive.ubuntu.com/ubuntu/pool/main/libw/libwebp/libwebpmux3_0.6.1-2_amd64.deb
    wget http://archive.ubuntu.com/ubuntu/pool/main/libw/libwebp/libwebp6_0.6.1-2_amd64.deb
    sudo dpkg -i *.deb

    . $TRAVIS_BUILD_DIR/build/install-vips.sh \
      --disable-dependency-tracking
fi
