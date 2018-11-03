#pragma once

#include <string.h>
#include "Types.h"

class ExecutionScript
{
public:

	// Constants

	static const int32 ScriptHashLength = 20;
	const int32 ScriptLength;

private:

	// Is Script hash calculated?

	bool _isScriptHashCalculated;
	byte _scriptHash[ScriptHashLength];

public:

	byte* Content;

	// Get ScriptHash

	int32 GetScriptHash(byte* hash);
	bool  IsTheSameHash(byte* hash, int32 length);

	// Constructor

	inline ExecutionScript(byte* script, int32 scriptLength) :
		_isScriptHashCalculated(false), 
		ScriptLength(scriptLength)
	{
		// Copy script

		this->Content = new byte[scriptLength];
		memcpy(this->Content, script, scriptLength);
	}

	// Destructor

	inline ~ExecutionScript()
	{
		delete(this->Content);
	}
};