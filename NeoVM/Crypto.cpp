#include "Crypto.h"
#include "SHA1.h"
#include "SHA256.h"
#include "RIPEMD160.h"

void Crypto::ComputeHash160(byte* data, int32 length, byte *output)
{
	byte digest[SHA256::DIGEST_SIZE];

	// First SHA256

	SHA256 ctx = SHA256();
	ctx.init();
	ctx.update(&data[0], length);
	ctx.final(digest);

	// Then RIPEMD160

	ripemd160(digest, SHA256::DIGEST_SIZE, output);
}

void Crypto::ComputeHash256(byte* data, int32 length, byte *output)
{
	byte digest[SHA256::DIGEST_SIZE];

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

void Crypto::ComputeSHA256(byte* data, int32 length, byte *output)
{
	SHA256 ctx = SHA256();
	ctx.init();
	ctx.update(&data[0], length);
	ctx.final(output);
}

void Crypto::ComputeSHA1(byte* data, int32 length, byte *output)
{
	SHA1 ctx = SHA1();
	ctx.addBytes((const char*)data, length);
	ctx.getDigest(output);
}