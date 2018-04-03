#pragma once

#include "Types.h"

class Crypto
{
public:

	static const int32 SHA1_LENGTH = 20U;
	static const int32 SHA256_LENGTH = 32U;
	static const int32 HASH160_LENGTH = 20U;
	static const int32 HASH256_LENGTH = SHA256_LENGTH;

	static void ComputeSHA1(byte* data, int32 length, byte *output);
	static void ComputeSHA256(byte* data, int32 length, byte *output);
	static void ComputeHash160(byte* data, int32 length, byte *output);
	static void ComputeHash256(byte* data, int32 length, byte *output);

	static bool VerifySignature(byte* data, int32 dataSize, byte* signature, int32 signatureSize, byte* pubKey, int32 pubKeySize);
};