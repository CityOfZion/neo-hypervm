#!/bin/bash


echo "building openssl 1.1.0i (for linux only)"
# ===============
# getting openssl
# ===============
wget -nc https://www.openssl.org/source/openssl-1.1.0i.tar.gz
rm -rf openssl-1.1.0i
tar -zxvf openssl-1.1.0i.tar.gz
rm -rf openssl
mv openssl-1.1.0i openssl
# ================
# building openssl
# ================
mkdir tmp_build
cd tmp_build
../openssl/config
make && make test && cp libcrypto.a ../liblinux-openssl-110-crypto-x64.a
cd ..
echo "finished building openssl 1.1.0i (for linux only)"

: <<'END'

echo "building openssl LTS 1.0.2p (linux + cross compiling for windows)"
#apt install mingw-64
wget -nc https://www.openssl.org/source/openssl-1.0.2p.tar.gz
tar -zxvf openssl-1.0.2p.tar.gz
cd openssl-1.0.2p
# building for linux (important to be made first! it generates apps/openssl)
./config
make && make test && cp libcrypto.a ../liblinux-openssl-crypto-x64.a
# cross building for windows
./Configure mingw64 shared --cross-compile-prefix=x86_64-w64-mingw32-
make clean # important to clean linux mess, but keep apps/openssl
make && make test && cp libcrypto.dll.a ../libwindows-openssl-crypto-x64.lib
make clean
cd ..
echo "finished building openssl LTS 1.0.2p (linux + cross compiling for windows)"

echo "building libressl 2.7.4 (for linux only)"
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
make check && cp crypto/.libs/libcrypto.a ../liblinux-libressl-crypto-x64.a
make clean
cd ..
echo "finished building libressl 2.7.4 (for linux only)"

END


mkdir openssl-1.1.0i
mv openssl/include openssl-1.1.0i/   # include files
mv tmp_build/include/openssl/* openssl-1.1.0i/include/openssl/   # just opensslconf.h
mv liblinux-openssl-110-crypto-x64.a openssl-1.1.0i/liblinux-openssl-crypto-x86_64.a  # naming convention
rm -rf openssl
rm -rf tmp_build

: <<'END'

mkdir libs/openssl-lts
#remove symlinks
cd openssl-1.0.2p/include/openssl
for i in `ls`; do cp --remove-destination `readlink $i` $i; done
cd ../../..
mv openssl-1.0.2p/include libs/openssl-lts/
mv libwindows-openssl-crypto-x64.lib libs/openssl-lts/
mv liblinux-openssl-crypto-x64.a libs/openssl-lts/
rm -rf openssl-1.0.2p

mkdir libs/libressl
mv libressl-2.7.4/include libs/libressl/
mv liblinux-libressl-crypto-x64.a libs/libressl/
rm -rf libressl-2.7.4
rm -rf tmp_build

END
