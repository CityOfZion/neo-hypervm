#pragma once

#include "Types.h"
#include "IClaimable.h"

class ExecutionScript
{
private:

	// Is Script hash calculated?

	bool IsScriptHashCalculated;

public:

	// Constants

	static const int32 ScriptHashLength = 20;

	byte * Content;
	const int32 ScriptLength;
	byte ScriptHash[ScriptHashLength];

	// Get ScriptHash

	int32 GetScriptHash(byte* hash);
	bool IsTheSameHash(byte *hash, int32 length);

	// Claim

	static void UnclaimAndFree(ExecutionScript* &item);

	// Constructor

	ExecutionScript(byte * script, int32 scriptLength);

	// Destructor

	~ExecutionScript();
};