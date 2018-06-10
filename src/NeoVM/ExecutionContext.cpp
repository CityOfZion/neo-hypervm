#include "ExecutionContext.h"
#include "Crypto.h"
#include <cstring>

ExecutionContext::ExecutionContext(ExecutionScript* script, int32 instructorPointer, int32 rvcount) :
	IClaimable(),
	ScriptLength(script->ScriptLength),
	RVCount(rvcount),
	Script(script),
	InstructionPointer(instructorPointer),
	AltStack(new StackItems()),
	EvaluationStack(new StackItems())
{
	script->Claim();
}

bool ExecutionContext::ReadUInt8(byte &ret)
{
	byte data[1];

	if (Read(data, 1) != 1)
		return false;

	ret = data[0];
	return true;
}

bool ExecutionContext::ReadUInt16(uint16 &ret)
{
	byte data[2];

	if (Read(data, 2) != 2)
		return false;

	ret = (uint16)((int32)data[0] | (int32)data[1] << 8);
	return true;
}

bool ExecutionContext::ReadInt16(int16 &ret)
{
	byte data[2];

	if (Read(data, 2) != 2)
		return false;

	ret = (int16)((int32)data[0] | (int32)data[1] << 8);
	return true;
}

bool ExecutionContext::ReadUInt32(uint32 &ret)
{
	byte data[4];

	if (Read(data, 4) != 4)
		return false;

	ret = (uint32)((int32)data[0] | (int32)data[1] << 8 | (int32)data[2] << 16 | (int32)data[3] << 24);
	return true;
}

bool ExecutionContext::ReadInt32(int32 &ret)
{
	byte data[4];

	if (Read(data, 4) != 4)
		return false;

	ret = (int32)((int32)data[0] | (int32)data[1] << 8 | (int32)data[2] << 16 | (int32)data[3] << 24);
	return true;
}

bool ExecutionContext::ReadInt64(int64 &ret)
{
	byte data[8];

	if (Read(data, 8) != 8)
		return false;

	uint32 a = ((uint32)((int32)data[0] | (int32)data[1] << 8 | (int32)data[2] << 16 | (int32)data[3] << 24));
	uint32 b = ((uint32)((int32)data[4] | (int32)data[5] << 8 | (int32)data[6] << 16 | (int32)data[7] << 24));

	ret = (int64)((uint64)b << 32 | (uint64)a);
	return true;
}

bool ExecutionContext::ReadUInt64(uint64 &ret)
{
	byte data[8];

	if (Read(data, 8) != 8)
		return false;

	uint32 a = ((uint32)((int32)data[0] | (int32)data[1] << 8 | (int32)data[2] << 16 | (int32)data[3] << 24));
	uint32 b = ((uint32)((int32)data[4] | (int32)data[5] << 8 | (int32)data[6] << 16 | (int32)data[7] << 24));

	ret = ((uint64)b << 32 | (uint64)a);
	return true;
}

bool ExecutionContext::ReadVarBytes(uint32 &ret, uint32 max)
{
	byte fb = 0;

	if (!this->ReadUInt8(fb))
		return false;

	if (fb == 0xFD)
	{
		uint16 v = 0;

		if (!this->ReadUInt16(v))
			return false;

		ret = v;
	}
	else if (fb == 0xFE)
	{
		uint32 v = 0;

		if (!this->ReadUInt32(v))
			return false;

		ret = v;
	}
	else if (fb == 0xFF)
	{
		uint64 v = 0;

		// Never is needed read more than an integer

		if (!this->ReadUInt64(v) || v > 0xFFFFFFFF)
			return false;

		ret = (uint32)(v & 0xffffffff);
	}
	else
	{
		ret = fb;
	}

	return ret <= max;
}

int32 ExecutionContext::Read(byte* data, int32 length)
{
	if (data == NULL)
	{
		// Seek
		this->InstructionPointer += length;
		return length;
	}

	int32 read = 0;

	for (int32 x = 0; x < length && this->InstructionPointer < this->ScriptLength; ++x)
	{
		data[x] = this->Script->Content[this->InstructionPointer];
		this->InstructionPointer++;
		read++;
	}

	return read;
}

EVMOpCode ExecutionContext::GetNextInstruction()
{
	if (this->InstructionPointer >= this->ScriptLength)
	{
		return EVMOpCode::RET;
	}
	else
	{
		return (EVMOpCode)this->Script->Content[this->InstructionPointer];
	}
}

EVMOpCode ExecutionContext::ReadNextInstruction()
{
	if (this->InstructionPointer >= this->ScriptLength)
	{
		return EVMOpCode::RET;
	}
	else
	{
		EVMOpCode ret = (EVMOpCode)this->Script->Content[this->InstructionPointer];
		this->InstructionPointer++;
		return ret;
	}
}

int32 ExecutionContext::GetScriptHash(byte* hash)
{
	return this->Script->GetScriptHash(hash);
}

void ExecutionContext::Seek(int32 position)
{
	this->InstructionPointer = position;
}

void ExecutionContext::Free(ExecutionContext* &item)
{
	if (item != NULL && item->IsUnClaimed())
	{
		delete(item);
		item = NULL;
	}
}

void ExecutionContext::UnclaimAndFree(ExecutionContext* &item)
{
	if (item != NULL && item->UnClaim())
	{
		delete(item);
		item = NULL;
	}
}

ExecutionContext::~ExecutionContext()
{
	delete(this->EvaluationStack);
	delete(this->AltStack);

	ExecutionScript::UnclaimAndFree(this->Script);
}