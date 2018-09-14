#!/bin/bash

SCRIPTPATH=$(pwd)

cd $SCRIPTPATH/../tests/

if [[ $TRAVIS_OS_NAME == 'osx' ]]; then
    export NEO_VM_PATH=$SCRIPTPATH/../src/bin/Neo.HyperVM.dylib
else
    export NEO_VM_PATH=$SCRIPTPATH/../src/bin/Neo.HyperVM.so
fi

echo "************************"
echo "**     BENCHMARKS     **"
echo "************************"

dotnet build --configuration Release Neo.HyperVM.Benchmarks
dotnet run --configuration Release --project Neo.HyperVM.Benchmarks
