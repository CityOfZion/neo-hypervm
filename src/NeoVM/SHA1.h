#pragma once

/*
http://www.tamale.net/sha1/

Copyright (c) 2005 Michael D. Leonhard

http://tamale.net/

This file is licensed under the terms described in the
accompanying LICENSE file.
*/

#include "Types.h"

class SHA1
{
private:
	// fields
	uint32 H0, H1, H2, H3, H4;
	byte bytes[64];
	int32 unprocessedBytes;
	uint32 size;
	void process();

public:

	SHA1();
	~SHA1();

	void addBytes(const byte* data, int32 num);
	void getDigest(byte* diggest);

	// utility methods
	static uint32 lrot(uint32 x, int32 bits);
	static void storeBigEndianUint32(byte* byte, uint32 num);
};