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

make
