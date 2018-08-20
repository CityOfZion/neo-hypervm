#!/bin/bash

SCRIPT=$(readlink -f "$0")
SCRIPTPATH=$(dirname "$SCRIPT")

cd $SCRIPTPATH/../tests/

export NEO_VM_PATH=$SCRIPTPATH/../src/bin/NeoVM.so

echo "************************"
echo "**     BENCHMARKS     **"
echo "************************"

dotnet build --configuration Release Neo.HyperVM.Benchmarks
dotnet run --configuration Release --project Neo.HyperVM.Benchmarks
