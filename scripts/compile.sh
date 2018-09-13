#!/bin/bash

SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")

cd $SCRIPTPATH/../src

echo "************************"
echo "**     MAKE CLEAN     **"
echo "************************"

make clean

echo "************************"
echo "**        MAKE        **"
echo "************************"

if [[ $TRAVIS_OS_NAME == 'osx' ]]; then
    make --file=Makefile-osx
else
    make
fi