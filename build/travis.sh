#!/usr/bin/env bash

set -e

if [[ $TRAVIS_OS_NAME == 'osx' ]]; then
    sw_vers

    # oclint cask conflicts with gcc formula, see:
    # https://github.com/travis-ci/travis-ci/issues/8826
    brew cask uninstall oclint

    # install gcc
    # if there are conflicts, try overwriting the files
    # (these are in /usr/local anyway so it should be ok)
    brew install gcc || brew link --overwrite gcc

    # install optional add-ons for libvips
    brew install cfitsio gsl libmatio

    # install libvips (full-fat version)
    brew install vips \
      --env=std \
      --with-imagemagick \
      --without-graphicsmagick \
      --with-openexr \
      --with-openslide
else
    uname -a

    . $TRAVIS_BUILD_DIR/build/install-vips.sh \
      --without-python
fi