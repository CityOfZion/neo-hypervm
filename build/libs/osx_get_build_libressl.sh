#!/bin/bash


LIBRESSL_VER=libressl-2.7.4

echo "# Building $LIBRESSL_VER (for OSX)"
# ================
# getting libressl
# ================
wget -nc https://ftp.openbsd.org/pub/OpenBSD/LibreSSL/$LIBRESSL_VER.tar.gz
rm -rf libressl_src
tar -zxf $LIBRESSL_VER.tar.gz
rm -rf libressl_src
mv $LIBRESSL_VER libressl_src
# =================
# building libressl
# =================
mkdir tmp_build
cd tmp_build
../libressl_src/configure --prefix=/opt/libressl --with-openssldir=/System/Library/OpenSSL --with-enginesdir=/opt/libress
make depend
make check
cd ..
echo "# Finished building $LIBRESSL_VER (for OSX)"

mkdir libressl
mv libressl_src/include libressl/   # include files
mv tmp_build/include/openssl/* libressl/include/openssl/
mv tmp_build/crypto/.libs/libcrypto.a libressl/libosx-libressl-crypto-x86_64.a
rm -rf libressl_src
rm -rf tmp_build
