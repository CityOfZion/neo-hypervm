LIBRESSL_VER=libressl-2.7.4

echo "# Building $LIBRESSL_VER (linux cross compiling for windows)"
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
CC=x86_64-w64-mingw32-gcc
../libressl_src/configure --host=x86_64-w64-mingw32
make depend
make check
cd ..
echo "# Finished building $LIBRESSL_VER (for windows)"

mkdir libressl
mv libressl_src/include libressl/   # include files
mv tmp_build/include/openssl/* libressl/include/openssl/
mv tmp_build/crypto/.libs/libcrypto.dll.a libressl/libwindows-libressl-crypto-x86_64.lib
#rm -rf libressl_src
#rm -rf tmp_build