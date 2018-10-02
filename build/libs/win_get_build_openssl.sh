#!/bin/bash


OPENSSL_VER=openssl-1.1.0i

echo "# Building $OPENSSL_VER (linux cross compiling for windows)"
# ===============
# getting openssl
# ===============
wget -nc https://www.openssl.org/source/$OPENSSL_VER.tar.gz
rm -rf openssl_src
tar -zxf $OPENSSL_VER.tar.gz
rm -rf openssl_src
mv $OPENSSL_VER openssl_src
# ================
# building openssl
# ================
mkdir tmp_build
cd tmp_build
../openssl_src/Configure mingw64 enable-ec_nistp_64_gcc_128 no-ssl3 no-comp --cross-compile-prefix=x86_64-w64-mingw32-
make depend
make #&& make test
cd ..
echo "# Finished building $OPENSSL_VER (for windows)"

mkdir openssl
mv openssl_src/include openssl/   # include files
mv tmp_build/include/openssl/* openssl/include/openssl/
mv tmp_build/libcrypto.dll.a openssl/libwindows-openssl-crypto-x86_64.lib
rm -rf openssl_src
rm -rf tmp_build
