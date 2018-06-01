#pragma once

#include "Types.h"

class Crypto
{
public:

	// Constants

	static const int32 SHA1_LENGTH = 20U;
	static const int32 SHA256_LENGTH = 32U;
	static const int32 HASH160_LENGTH = 20U;
	static const int32 HASH256_LENGTH = SHA256_LENGTH;

	// Empty hashes

	static const byte EMPTY_HASH160[HASH160_LENGTH];
	static const byte EMPTY_HASH256[HASH256_LENGTH];
	static const byte EMPTY_SHA1[SHA1_LENGTH];
	static const byte EMPTY_SHA256[SHA256_LENGTH];

	// Methods

	static void ComputeSHA1(byte* data, int32 length, byte *output);
	static void ComputeSHA256(byte* data, int32 length, byte *output);
	static void ComputeHash160(byte* data, int32 length, byte *output);
	static void ComputeHash256(byte* data, int32 length, byte *output);


	// -1=ERROR , 0= False , 1=True 
	static int16 VerifySignature(byte* data, int32 dataLength, byte* signature, int32 signatureLength, byte* pubKey, int32 pubKeyLength);
};