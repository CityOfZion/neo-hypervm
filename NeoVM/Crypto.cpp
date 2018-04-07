#include "Crypto.h"
#include "SHA1.h"
#include "SHA256.h"
#include "RIPEMD160.h"
#include <cstring>

const byte Crypto::EMPTY_SHA1[] =
{
	0xda,0x39,0xa3,0xee,0x5e,0x6b,0x4b,0x0d,0x32,0x55,
	0xbf,0xef,0x95,0x60,0x18,0x90,0xaf,0xd8,0x07,0x09
};

const byte Crypto::EMPTY_HASH160[] =
{
	0xb4,0x72,0xa2,0x66,0xd0,0xbd,0x89,0xc1,0x37,0x06,
	0xa4,0x13,0x2c,0xcf,0xb1,0x6f,0x7c,0x3b,0x9f,0xcb
};

const byte Crypto::EMPTY_SHA256[] =
{
	0xe3,0xb0,0xc4,0x42,0x98,0xfc,0x1c,0x14,0x9a,0xfb,0xf4,0xc8,0x99,0x6f,0xb9,0x24,
	0x27,0xae,0x41,0xe4,0x64,0x9b,0x93,0x4c,0xa4,0x95,0x99,0x1b,0x78,0x52,0xb8,0x55
};

const byte Crypto::EMPTY_HASH256[] =
{
	0x5d,0xf6,0xe0,0xe2,0x76,0x13,0x59,0xd3,0x0a,0x82,0x75,0x05,0x8e,0x29,0x9f,0xcc,
	0x03,0x81,0x53,0x45,0x45,0xf5,0x5c,0xf4,0x3e,0x41,0x98,0x3f,0x5d,0x4c,0x94,0x56
};

bool Crypto::VerifySignature
(
	byte* data, int32 dataLength,
	byte* signature, int32 signatureLength,
	byte* pubKey, int32 pubKeyLength
)
{
	if (signatureLength <= 0)
		return false;

	int pubKeyIndex = 0;
	if (pubKeyLength == 33 && (pubKey[0] == 0x02 || pubKey[0] == 0x03))
	{
		/*
		try
		{
			pubkey = Cryptography.ECC.ECPoint.DecodePoint(pubkey, Cryptography.ECC.ECCurve.Secp256r1).EncodePoint(false).Skip(1).ToArray();
		}
		catch
		{
			return false;
		}
		*/
	}
	else if (pubKeyLength == 65 && pubKey[0] == 0x04)
	{
		pubKeyIndex = 1;
		pubKeyLength = 64;
	}
	else if (pubKeyLength != 64)
	{
		return false;
	}

	/*
	using (var ecdsa = ECDsa.Create(new ECParameters
		{
			Curve = ECCurve.NamedCurves.nistP256,
			Q = new ECPoint
		{
			X = pubkey.Take(32).ToArray(),
			Y = pubkey.Skip(32).ToArray()
		}
		}))
	{
		return ecdsa.VerifyData(message, signature, HashAlgorithmName.SHA256);
	}
	*/

	return false;
}

void Crypto::ComputeHash160(byte* data, int32 length, byte *output)
{
	if (length <= 0)
	{
		memcpy(output, Crypto::EMPTY_HASH160, Crypto::HASH160_LENGTH);
		return;
	}

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
	if (length <= 0)
	{
		memcpy(output, Crypto::EMPTY_HASH256, Crypto::HASH256_LENGTH);
		return;
	}

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
	if (length <= 0)
	{
		memcpy(output, Crypto::EMPTY_SHA256, Crypto::SHA256_LENGTH);
		return;
	}

	SHA256 ctx = SHA256();
	ctx.init();
	ctx.update(&data[0], length);
	ctx.final(output);
}

void Crypto::ComputeSHA1(byte* data, int32 length, byte *output)
{
	if (length <= 0)
	{
		memcpy(output, Crypto::EMPTY_SHA1, Crypto::SHA1_LENGTH);
		return;
	}

	SHA1 ctx = SHA1();
	ctx.addBytes((const byte*)data, length);
	ctx.getDigest(output);
}