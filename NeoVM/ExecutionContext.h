#pragma once
#include "EVMOpCode.h"

class ExecutionContext
{
public:

	// Constants

	static const int ScriptHashLength = 20;

	const bool IsPushOnly;
	const int ScriptLength;

	// Position

	int InstructionPointer;

	// Reads

	int Read(unsigned char * data, int length);

	bool ReadUInt8(unsigned __int8 &ret);
	bool ReadUInt16(unsigned __int16 &ret);
	bool ReadInt16(__int16 &ret);
	bool ReadUInt32(unsigned __int32 &ret);
	bool ReadInt32(__int32 &ret);
	bool ReadUInt64(unsigned __int64 &ret);
	bool ReadInt64(__int64 &ret);
	bool ReadVarBytes(__int64 &ret, __int64 max);

	// Get/Read next instruction

	EVMOpCode GetNextInstruction();
	EVMOpCode ReadNextInstruction();
	void Seek(int position);

	// Get script hash

	int GetScriptHash(unsigned char* hash);

	// Clone execution context

	ExecutionContext* Clone();

	// Constructor

	ExecutionContext(unsigned char* script, int scriptLength, bool pushOnly, int instructorPointer);

	// Destructor

	~ExecutionContext();

private:

	// Script

	unsigned char* Script;
	bool IsScriptHashCalculated;
	unsigned char ScriptHash[ScriptHashLength];
};