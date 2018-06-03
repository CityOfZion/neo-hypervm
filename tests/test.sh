#!/bin/bash

echo "************************"
echo "**     MAKE CLEAN     **"
echo "************************"

make -C ../src/ -f ../src/Makefile clean

echo "************************"
echo "**        MAKE        **"
echo "************************"

make -C ../src/ -f ../src/Makefile

# TODO: Change local path

export NEO_HYPERVM_PATH=/mnt/c/Sources/Neo/neo-hypervm/src/bin/NeoVM.so

echo "************************"
echo "**      UNIT TEST     **"
echo "************************"

dotnet test ./NeoVM.Interop.Tests