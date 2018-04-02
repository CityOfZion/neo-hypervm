#pragma once

// https://github.com/sipa/Coin25519/blob/master/src/crypto/ripemd160.h

#include <stdint.h>
#include "Types.h"

typedef struct 
{
	uint64 length;
	union 
	{
		uint32 w[16];
		byte b[64];
	} 
	buf;
	uint32 h[5];
	byte bufpos;
} 
ripemd160_state;

void ripemd160_init(ripemd160_state *self);
void ripemd160_process(ripemd160_state *self, const byte *p, uint32 length);
void ripemd160_done(ripemd160_state *self, byte *out);
void ripemd160(const void* in, uint32 length, void* out);
