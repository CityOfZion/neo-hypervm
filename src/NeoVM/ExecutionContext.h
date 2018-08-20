#pragma once

#include "EVMOpCode.h"
#include "Types.h"
#include "StackItems.h"
#include "ExecutionScript.h"

class ExecutionContext :public IClaimable
{
public:

	// Consts

	const int32 ScriptLength;
	const int32 RVCount;

	// Script

	ExecutionScript* Script;
	int32 InstructionPointer;

	// Stacks

	StackItems AltStack;
	StackItems EvaluationStack;

	// Reads

	int32 Read(byte* data, int32 length);

	bool ReadUInt8(byte &ret);
	bool ReadUInt16(uint16 &ret);
	bool ReadInt16(int16 &ret);
	bool ReadUInt32(uint32 &ret);
	bool ReadInt32(int32 &ret);
	bool ReadUInt64(uint64 &ret);
	bool ReadInt64(int64 &ret);
	bool ReadVarBytes(uint32 &ret, uint32 max);

	// Get/Read next instruction

	EVMOpCode GetNextInstruction();
	EVMOpCode ReadNextInstruction();

	inline void Seek(int32 position)
	{
		this->InstructionPointer = position;
	}

	// Get script hash

	inline int32 GetScriptHash(byte* hash)
	{
		return this->Script->GetScriptHash(hash);
	}

	// Constructor

	ExecutionContext(ExecutionScript* script, int32 instructorPointer, int32 rvcount);

	// Destructor

	inline ~ExecutionContext()
	{
		this->EvaluationStack.Clear();
		this->AltStack.Clear();

		ExecutionScript::UnclaimAndFree(this->Script);
	}

	// Claims

	static void Free(ExecutionContext* &item);
	static void UnclaimAndFree(ExecutionContext* &item);
};