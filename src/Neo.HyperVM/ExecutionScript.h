#pragma once

#include "Types.h"
#include "IClaimable.h"

class ExecutionScript : public IClaimable
{
private:

	// Is Script hash calculated?

	bool _isScriptHashCalculated;

public:

	// Constants

	static const int32 ScriptHashLength = 20;

	byte* Content;
	const int32 ScriptLength;
	byte ScriptHash[ScriptHashLength];

	// Get ScriptHash

	int32 GetScriptHash(byte* hash);
	bool IsTheSameHash(byte* hash, int32 length);

	// Constructor

	ExecutionScript(byte* script, int32 scriptLength);

	// Destructor

	inline ~ExecutionScript()
	{
		delete(this->Content);
	}

	// Claims

	static void Free(ExecutionScript* &item);
	static void UnclaimAndFree(ExecutionScript* &item);
};
