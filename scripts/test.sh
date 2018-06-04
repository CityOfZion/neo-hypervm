#!/bin/bash

./compile.sh

SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")

cd $SCRIPTPATH/../tests/

# TODO: Change local path

export NEO_HYPERVM_PATH=$SCRIPTPATH/../src/bin/NeoVM.so

echo "************************"
echo "**      UNIT TEST     **"
echo "************************"

dotnet test NeoVM.Interop.Tests