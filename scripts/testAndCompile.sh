#!/bin/bash

./compile.sh

SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")

cd $SCRIPTPATH/../tests/

if [[ $TRAVIS_OS_NAME == 'osx' ]]; then
    export NEO_VM_PATH=$SCRIPTPATH/../src/bin/Neo.HyperVM.dylib
else
    export NEO_VM_PATH=$SCRIPTPATH/../src/bin/Neo.HyperVM.so
fi

echo "************************"
echo "**      UNIT TEST     **"
echo "************************"

dotnet test NeoSharp.VM.Interop.Tests --verbosity n
