#include "ExecutionScript.h"
#include "Crypto.h"
#include <cstring>

ExecutionScript::ExecutionScript(byte * script, int32 scriptLength) :
	IsScriptHashCalculated(false),
	ScriptLength(scriptLength)
{
	// Copy script

	this->Content = new byte[scriptLength];
	memcpy(this->Content, script, scriptLength);
}

int32 ExecutionScript::GetScriptHash(byte* hash)
{
	if (!this->IsScriptHashCalculated)
	{
		// Compute script hash

		this->IsScriptHashCalculated = true;
		Crypto::ComputeHash160(this->Content, this->ScriptLength, &this->ScriptHash[0]);
	}

	memcpy(hash, this->ScriptHash, this->ScriptHashLength);
	return this->ScriptHashLength;
}

bool ExecutionScript::IsTheSameHash(byte *hash, int32 length)
{
	if (length != ScriptHashLength) return false;

	if (!this->IsScriptHashCalculated)
	{
		// Compute script hash

		this->IsScriptHashCalculated = true;
		Crypto::ComputeHash160(this->Content, this->ScriptLength, &this->ScriptHash[0]);
	}

	for (int32 x = 0; x < ScriptHashLength; x++)
		if (this->ScriptHash[x] != hash[x])
			return false;

	return true;
}

ExecutionScript::~ExecutionScript()
{
	delete(this->Content);
}