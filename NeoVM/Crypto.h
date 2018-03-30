#pragma once

class Crypto
{
public:

	static const int SHA1_LENGTH = 20U;
	static const int SHA256_LENGTH = 32U;
	static const int HASH160_LENGTH = 20U;
	static const int HASH256_LENGTH = SHA256_LENGTH;

	static void ComputeSHA1(unsigned char* data, int length, unsigned char *output);
	static void ComputeSHA256(unsigned char* data, int length, unsigned char *output);
	static void ComputeHash160(unsigned char* data, int length, unsigned char *output);
	static void ComputeHash256(unsigned char* data, int length, unsigned char *output);
};