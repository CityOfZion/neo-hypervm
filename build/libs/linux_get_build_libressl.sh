#!/bin/bash


LIBRESSL_VER=libressl-2.7.4

echo "# Building $LIBRESSL_VER (for Linux)"
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
../libressl_src/configure
make check
cd ..
echo "# Finished building $LIBRESSL_VER (for Linux)"

mkdir libressl
mv libressl_src/include libressl/   # include files
mv tmp_build/include/openssl/* libressl/include/openssl/
mv tmp_build/crypto/.libs/libcrypto.a libressl/liblinux-libressl-crypto-x86_64.a
rm -rf libressl_src
rm -rf tmp_build
