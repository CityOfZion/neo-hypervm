#include "Crypto.h"
#include "SHA1.h"
#include "SHA256.h"
#include "RIPEMD160.h"

void Crypto::ComputeHash160(unsigned char* data, int length, unsigned char *output)
{
	unsigned char digest[SHA256::DIGEST_SIZE];

	// First SHA256

	SHA256 ctx = SHA256();
	ctx.init();
	ctx.update(&data[0], length);
	ctx.final(digest);

	// Then RIPEMD160

	ripemd160(digest, SHA256::DIGEST_SIZE, output);
}

void Crypto::ComputeHash256(unsigned char* data, int length, unsigned char *output)
{
	unsigned char digest[SHA256::DIGEST_SIZE];

	// First SHA256

	SHA256 ctx = SHA256();
	ctx.init();
	ctx.update(&data[0], length);
	ctx.final(digest);

	// Then SHA256 Again

	ctx.init();
	ctx.update(&digest[0], SHA256::DIGEST_SIZE);
	ctx.final(output);
}

void Crypto::ComputeSHA256(unsigned char* data, int length, unsigned char *output)
{
	SHA256 ctx = SHA256();
	ctx.init();
	ctx.update(&data[0], length);
	ctx.final(output);
}

void Crypto::ComputeSHA1(unsigned char* data, int length, unsigned char *output)
{
	SHA1 ctx = SHA1();
	ctx.addBytes((const char*)data, length);
	ctx.getDigest(output);
}