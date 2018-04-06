#include "SHA1.h"

/*
Copyright (c) 2005 Michael D. Leonhard

http://tamale.net/

This file is licensed under the terms described in the
accompanying LICENSE file.
*/

#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <assert.h>

// circular left bit rotation.  MSB wraps around to LSB
uint32 SHA1::lrot(uint32 x, int32 bits)
{
	return (x << bits) | (x >> (32 - bits));
};

// Save a 32-bit unsigned integer to memory, in big-endian order
void SHA1::storeBigEndianUint32(byte* byte, uint32 num)
{
	assert(byte);
	byte[0] = (unsigned char)(num >> 24);
	byte[1] = (unsigned char)(num >> 16);
	byte[2] = (unsigned char)(num >> 8);
	byte[3] = (unsigned char)num;
}


// Constructor *******************************************************
SHA1::SHA1()
{
	// make sure that the data type is the right size
	assert(sizeof(uint32) * 5 == 20);

	// initialize
	H0 = 0x67452301;
	H1 = 0xefcdab89;
	H2 = 0x98badcfe;
	H3 = 0x10325476;
	H4 = 0xc3d2e1f0;
	unprocessedBytes = 0;
	size = 0;
}

// Destructor ********************************************************
SHA1::~SHA1()
{
	// erase data
	H0 = H1 = H2 = H3 = H4 = 0;
	for (int c = 0; c < 64; c++) bytes[c] = 0;
	unprocessedBytes = size = 0;
}

// process ***********************************************************
void SHA1::process()
{
	assert(unprocessedBytes == 64);
	//printf( "process: " ); hexPrinter( bytes, 64 ); printf( "\n" );
	int32 t;
	uint32 a, b, c, d, e, K, f, W[80];
	// starting values
	a = H0;
	b = H1;
	c = H2;
	d = H3;
	e = H4;
	// copy and expand the message block
	for (t = 0; t < 16; t++) W[t] = (bytes[t * 4] << 24)
		+ (bytes[t * 4 + 1] << 16)
		+ (bytes[t * 4 + 2] << 8)
		+ bytes[t * 4 + 3];
	for (; t < 80; t++) W[t] = lrot(W[t - 3] ^ W[t - 8] ^ W[t - 14] ^ W[t - 16], 1);

	/* main loop */
	uint32 temp;
	for (t = 0; t < 80; t++)
	{
		if (t < 20) {
			K = 0x5a827999;
			f = (b & c) | ((b ^ 0xFFFFFFFF) & d);//TODO: try using ~
		}
		else if (t < 40) {
			K = 0x6ed9eba1;
			f = b ^ c ^ d;
		}
		else if (t < 60) {
			K = 0x8f1bbcdc;
			f = (b & c) | (b & d) | (c & d);
		}
		else {
			K = 0xca62c1d6;
			f = b ^ c ^ d;
		}
		temp = lrot(a, 5) + f + e + W[t] + K;
		e = d;
		d = c;
		c = lrot(b, 30);
		b = a;
		a = temp;
		//printf( "t=%d %08x %08x %08x %08x %08x\n",t,a,b,c,d,e );
	}
	/* add variables */
	H0 += a;
	H1 += b;
	H2 += c;
	H3 += d;
	H4 += e;
	//printf( "Current: %08x %08x %08x %08x %08x\n",H0,H1,H2,H3,H4 );
	/* all bytes have been processed */
	unprocessedBytes = 0;
}

// addBytes **********************************************************
void SHA1::addBytes(const byte* data, int32 num)
{
	// add these bytes to the running total
	size += num;
	// repeat until all data is processed
	while (num > 0)
	{
		// number of bytes required to complete block
		int needed = 64 - unprocessedBytes;
		assert(needed > 0);
		// number of bytes to copy (use smaller of two)
		int toCopy = (num < needed) ? num : needed;
		// Copy the bytes
		memcpy(bytes + unprocessedBytes, data, toCopy);
		// Bytes have been copied
		num -= toCopy;
		data += toCopy;
		unprocessedBytes += toCopy;

		// there is a full block
		if (unprocessedBytes == 64) process();
	}
}

// digest ************************************************************
void SHA1::getDigest(byte * digest)
{
	// save the message size
	uint32 totalBitsL = size << 3;
	uint32 totalBitsH = size >> 29;
	
	// add 0x80 to the message
	const byte b080 = 0x80;
	addBytes(&b080, 1);

	byte footer[64] = {
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	// block has no room for 8-byte filesize, so finish it
	if (unprocessedBytes > 56)
		addBytes((byte*)footer, 64 - unprocessedBytes);
	assert(unprocessedBytes <= 56);
	// how many zeros do we need
	int neededZeros = 56 - unprocessedBytes;
	// store file size (in bits) in big-endian format
	storeBigEndianUint32(footer + neededZeros, totalBitsH);
	storeBigEndianUint32(footer + neededZeros + 4, totalBitsL);
	// finish the final block
	addBytes((byte*)footer, neededZeros + 8);

	// copy the digest bytes
	storeBigEndianUint32(digest, H0);
	storeBigEndianUint32(digest + 4, H1);
	storeBigEndianUint32(digest + 8, H2);
	storeBigEndianUint32(digest + 12, H3);
	storeBigEndianUint32(digest + 16, H4);
}