#!/bin/bash
echo "building openssl"
# ===============
# getting openssl
# ===============
wget -nc https://www.openssl.org/source/openssl-1.1.0i.tar.gz
tar -zxvf openssl-1.1.0i.tar.gz
#mv openssl-1.1.0i openssl
# ================
# building openssl
# ================
mkdir tmp_build
cd tmp_build
../openssl-1.1.0i/config
make
make test
#sudo make install # install in system
# ==============================
# copying openssl static library
# ==============================
cp libcrypto.a ../liblinux-openssl-crypto-x64.a
make clean
cd ..
echo "finished building openssl"

echo "building libressl"
# ===============
# getting libressl
# ===============
wget -nc https://ftp.openbsd.org/pub/OpenBSD/LibreSSL/libressl-2.7.4.tar.gz
tar -zxvf libressl-2.7.4.tar.gz
#mv libressl-2.7.4 libressl
# ================
# building openssl
# ================
cd libressl-2.7.4
./configure
make check
#sudo make install # install in system
# ==============================
# copying openssl static library
# ==============================
cp crypto/.libs/libcrypto.a ../liblinux-libressl-crypto-x64.a
make clean
cd ..
echo "finished building libressl"

mkdir libs/openssl
mv openssl-1.1.0i/include libs/openssl/
mv liblinux-openssl-crypto-x64.a libs/openssl/
rm -rf openssl-1.1.0i

mkdir libs/libressl
mv libressl-2.7.4/include libs/libressl/
mv liblinux-libressl-crypto-x64.a libs/libressl/
rm -rf libressl-2.7.4
rm -rf tmp_build
