#include "Crypto.h"
#include <cstring>
#include <openssl/ec.h>      // for EC_GROUP_new_by_curve_name, EC_GROUP_free, EC_KEY_new, EC_KEY_set_group, EC_KEY_generate_key, EC_KEY_free
#include <openssl/ecdsa.h>   // for ECDSA_do_sign, ECDSA_do_verify
#include <openssl/obj_mac.h> // for NID_secp192k1
#include <openssl/sha.h>
#include <openssl/ripemd.h>

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

int16 Crypto::VerifySignature
(
	byte* data, int32 dataLength,
	byte* signature, int32 signatureLength,
	byte* pubKey, int32 pubKeyLength
)
{
	if (signatureLength <= 0)
		return -1;

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
		return -1;
	}
	else if (pubKeyLength == 65 && pubKey[0] == 0x04)
	{
		pubKeyIndex = 1;
		pubKeyLength = 64;
	}
	else if (pubKeyLength != 64)
	{
		return -1;
	}

	int32 ret = -1;

	EC_GROUP *ecgroup = EC_GROUP_new_by_curve_name(NID_X9_62_prime256v1);
	if (ecgroup != NULL)
	{
		EC_KEY *eckey = EC_KEY_new_by_curve_name(NID_X9_62_prime256v1);
		if (eckey != NULL)
		{
			EC_POINT *pub = EC_POINT_new(ecgroup);
			int gen_status = EC_KEY_set_public_key(eckey, pub);

			if (pub != NULL)
			{
				if (gen_status == 0x01)
				{
					gen_status = EC_KEY_generate_key(eckey);
					if (gen_status == 0x01)
					{
						//ECDSA_SIG *sig = ECDSA_SIG_new();
						//i2d_ECDSA_SIG(sig, &signature);
						ECDSA_SIG *sig = d2i_ECDSA_SIG(NULL, (const unsigned char **)&signature, signatureLength);

						//	ECDSA_do_sign(data, dataLength, eckey);
						if (sig != NULL)
						{
							ret = ECDSA_do_verify(data, dataLength, sig, eckey);
							ECDSA_SIG_free(sig);
						}
					}
				}
				EC_POINT_free(pub);
			}
			EC_KEY_free(eckey);
		}
		EC_GROUP_free(ecgroup);
	}

	return ret ? 0x01 : 0x00;
}

void Crypto::ComputeHash160(byte* data, int32 length, byte *output)
{
	if (length <= 0)
	{
		memcpy(output, Crypto::EMPTY_HASH160, Crypto::HASH160_LENGTH);
		return;
	}

	byte digest[SHA256_DIGEST_LENGTH];

	// First SHA256

	ComputeSHA256(data, length, digest);

	// Then RIPEMD160

	RIPEMD160_CTX c;

	RIPEMD160_Init(&c);
	RIPEMD160_Update(&c, digest, SHA256_DIGEST_LENGTH);
	RIPEMD160_Final(output, &c);
	OPENSSL_cleanse(&c, sizeof(c));
}

void Crypto::ComputeHash256(byte* data, int32 length, byte *output)
{
	if (length <= 0)
	{
		memcpy(output, Crypto::EMPTY_HASH256, Crypto::HASH256_LENGTH);
		return;
	}

	byte digest[SHA256_LENGTH];

	// First SHA256

	ComputeSHA256(data, length, digest);

	// Then SHA256 Again

	ComputeSHA256(digest, SHA256_LENGTH, output);
}

void Crypto::ComputeSHA256(byte* data, int32 length, byte *output)
{
	if (length <= 0)
	{
		memcpy(output, Crypto::EMPTY_SHA256, Crypto::SHA256_LENGTH);
		return;
	}

	SHA256_CTX c;
	static unsigned char m[SHA256_LENGTH];

	SHA256_Init(&c);
	SHA256_Update(&c, data, length);
	SHA256_Final(output, &c);
	OPENSSL_cleanse(&c, sizeof(c));
}

void Crypto::ComputeSHA1(byte* data, int32 length, byte *output)
{
	if (length <= 0)
	{
		memcpy(output, Crypto::EMPTY_SHA1, Crypto::SHA1_LENGTH);
		return;
	}

	SHA_CTX c;
	static unsigned char m[SHA1_LENGTH];

	SHA1_Init(&c);
	SHA1_Update(&c, data, length);
	SHA1_Final(output, &c);
	OPENSSL_cleanse(&c, sizeof(c));
}