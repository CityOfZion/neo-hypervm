#!/bin/bash


echo "building openssl 1.1.0i (for OSX)"
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
../openssl/Configure darwin64-x86_64-cc shared enable-ec_nistp_64_gcc_128 no-ssl3 no-comp
make depend
make && make test && cp libcrypto.a ../libosx-openssl-110-crypto-x64.a
cd ..
echo "finished building openssl 1.1.0i (for OSX)"

mkdir openssl-1.1.0i
mv openssl/include openssl-1.1.0i/   # include files
mv tmp_build/include/openssl/* openssl-1.1.0i/include/openssl/   # just opensslconf.h
mv libosx-openssl-110-crypto-x64.a openssl-1.1.0i/libosx-openssl-crypto-x86_64.a  # naming convention
rm -rf openssl
rm -rf tmp_build
