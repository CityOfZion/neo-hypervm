#!/bin/bash

SCRIPTPATH=$(pwd)

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
    sudo apt-get update
    sudo apt-get install binutils binutils-multiarch g++-multilib
    make
fi