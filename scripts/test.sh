#!/bin/bash

SCRIPTPATH=$(pwd)

if [[ $TRAVIS_OS_NAME == 'osx' ]]; then
    export NEO_VM_PATH=$SCRIPTPATH/../src/bin/Neo.HyperVM.dylib
else
    export NEO_VM_PATH=$SCRIPTPATH/../src/bin/Neo.HyperVM.so
fi

echo "************************"
echo "**      UNIT TEST     **"
echo "************************"

cd $SCRIPTPATH/../tests/
dotnet test NeoSharp.VM.Interop.Tests --verbosity n
