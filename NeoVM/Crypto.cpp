#include "Crypto.h"
#include "SHA1.h"
#include "SHA256.h"
#include "RIPEMD160.h"

bool Crypto::VerifySignature
(
	byte* data, int32 dataLength,
	byte* signature, int32 signatureSize,
	byte* pubKey, int32 pubKeySize
)
{
	int pubKeyIndex = 0;
	if (pubKeySize == 33 && (pubKey[0] == 0x02 || pubKey[0] == 0x03))
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
	else if (pubKeySize == 65 && pubKey[0] == 0x04)
	{
		pubKeyIndex = 1;
		pubKeySize = 64;
	}
	else if (pubKeySize != 64)
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
	ctx.addBytes((const byte*)data, length);
	ctx.getDigest(output);
}