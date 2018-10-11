#pragma once

#include "EVMOpCode.h"
#include "Types.h"
#include "StackItems.h"
#include "ExecutionScript.h"

class ExecutionContext :public IClaimable
{
private:

	ExecutionScript* _script;
	int32 _instructionIndex;
	byte* _instructionPointer;
	byte _buffer[8];

	const int32 _scriptLength;

public:

	const int32 RVCount;

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

	inline int32 GetInstructionPointer() const
	{
		return this->_instructionIndex;
	}

	inline EVMOpCode GetNextInstruction() const
	{
		if (this->_instructionIndex >= this->_scriptLength)
		{
			return EVMOpCode::RET;
		}

		return (EVMOpCode)*(this->_instructionPointer);
	}

	inline EVMOpCode ReadNextInstruction()
	{
		if (this->_instructionIndex >= this->_scriptLength)
		{
			return EVMOpCode::RET;
		}

		this->_instructionIndex++;
		return (EVMOpCode)*(this->_instructionPointer++);
	}

	inline ExecutionContext* Clone(int32 rvcount, int32 pcount)
	{
		auto clone = new ExecutionContext(this->_script, this->_instructionIndex, rvcount);

		this->EvaluationStack.SendTo(&clone->EvaluationStack, pcount);

		return clone;
	}

	inline bool CouldSeekFromHere(int32 offset) const
	{
		int32 newPos = this->_instructionIndex + offset;

		if (newPos < 0 || newPos > this->_scriptLength)
		{
			return false;
		}

		return true;
	}

	inline bool SeekFromHere(int32 offset)
	{
		int32 newPos = this->_instructionIndex + offset;

		if (newPos < 0 || newPos > this->_scriptLength)
		{
			return false;
		}

		this->_instructionIndex = newPos;
		this->_instructionPointer += offset;

		return true;
	}

	// Get script hash

	inline int32 GetScriptHash(byte* hash) const
	{
		return this->_script->GetScriptHash(hash);
	}

	// Constructor

	inline ExecutionContext(ExecutionScript* script, int32 instructorPointer, int32 rvcount) :
		IClaimable(),
		_script(script),
		_instructionIndex(instructorPointer),
		_instructionPointer(&script->Content[instructorPointer]),
		_buffer(),
		_scriptLength(script->ScriptLength),
		RVCount(rvcount),
		AltStack(),
		EvaluationStack()
	{
		script->Claim();
	}

	// Destructor

	inline ~ExecutionContext()
	{
		this->EvaluationStack.Clear();
		this->AltStack.Clear();

		ExecutionScript::UnclaimAndFree(this->_script);
	}

	// Claims

	static void Free(ExecutionContext* &item);
	static void UnclaimAndFree(ExecutionContext* &item);
};