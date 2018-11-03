#include "ExecutionScript.h"
#include "Crypto.h"

int32 ExecutionScript::GetScriptHash(byte* hash)
{
	if (!this->_isScriptHashCalculated)
	{
		// Compute script hash

		this->_isScriptHashCalculated = true;
		Crypto::ComputeHash160(this->Content, this->ScriptLength, &this->_scriptHash[0]);
	}

	memcpy(hash, this->_scriptHash, this->ScriptHashLength);
	return this->ScriptHashLength;
}

bool ExecutionScript::IsTheSameHash(byte* hash, int32 length)
{
	if (length != this->ScriptHashLength) return false;

	if (!this->_isScriptHashCalculated)
	{
		// Compute script hash

		this->_isScriptHashCalculated = true;
		Crypto::ComputeHash160(this->Content, this->ScriptLength, &this->_scriptHash[0]);
	}

	for (int32 x = 0; x < this->ScriptHashLength; x++)
		if (this->_scriptHash[x] != hash[x])
			return false;

	return true;
}