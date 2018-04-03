#include "ExecutionContext.h"
#include "Crypto.h"
#include <cstring>

ExecutionContext::ExecutionContext(byte* script, int32 scriptLength, bool pushOnly, int32 instructorPointer)
	:ScriptLength(scriptLength), IsPushOnly(pushOnly), InstructionPointer(instructorPointer), IsScriptHashCalculated(false)
{
	// Copy script

	this->Script = new byte[scriptLength];
	memcpy(this->Script, script, scriptLength);
}

bool ExecutionContext::ReadUInt8(byte &ret)
{
	byte data[1];

	if (Read(data, 1) != 1)
		return false;

	ret = (byte)data[0];
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

	int32 a = ((int32)((int32)data[0] | (int32)data[1] << 8 | (int32)data[2] << 16 | (int32)data[3] << 24));
	int32 b = ((int32)((int32)data[4] | (int32)data[5] << 8 | (int32)data[6] << 16 | (int32)data[7] << 24));

	ret = (int64)(b << 32 | a);
	return true;
}

bool ExecutionContext::ReadUInt64(uint64 &ret)
{
	byte data[8];

	if (Read(data, 8) != 8)
		return false;

	uint32 a = ((uint32)((int32)data[0] | (int32)data[1] << 8 | (int32)data[2] << 16 | (int32)data[3] << 24));
	uint32 b = ((uint32)((int32)data[4] | (int32)data[5] << 8 | (int32)data[6] << 16 | (int32)data[7] << 24));

	ret = (uint64)(b << 32 | a);
	return true;
}

bool ExecutionContext::ReadVarBytes(int64 &ret, int64 max)
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

		if (!this->ReadUInt64(v))
			return false;

		ret = v;
	}
	else
	{
		ret = fb;
	}

	if (ret > max) return false;
	return true;
}

int32 ExecutionContext::Read(byte * data, int32 length)
{
	if (data == NULL)
	{
		// Seek
		this->InstructionPointer += length;
		return length;
	}

	int read = 0;

	for (int x = 0; x < length && this->InstructionPointer < this->ScriptLength; x++)
	{
		data[x] = this->Script[this->InstructionPointer];
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
		return (EVMOpCode)this->Script[this->InstructionPointer];
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
		EVMOpCode ret = (EVMOpCode)this->Script[this->InstructionPointer];
		this->InstructionPointer++;
		return ret;
	}
}

int32 ExecutionContext::GetScriptHash(byte* hash)
{
	if (!this->IsScriptHashCalculated)
	{
		this->IsScriptHashCalculated = true;
		Crypto::ComputeHash160(this->Script, this->ScriptLength, &this->ScriptHash[0]);
	}

	memcpy(hash, this->ScriptHash, this->ScriptHashLength);
	return this->ScriptHashLength;
}

void ExecutionContext::Seek(int32 position)
{
	this->InstructionPointer = position;
}

ExecutionContext* ExecutionContext::Clone()
{
	return new ExecutionContext(this->Script, this->ScriptLength, this->IsPushOnly, this->InstructionPointer);
}

ExecutionContext::~ExecutionContext()
{
	if (this->Script == NULL)
		return;

	delete[](this->Script);
	this->Script = NULL;
}