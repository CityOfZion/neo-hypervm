#include "ExecutionContext.h"
#include "Crypto.h"
#include <cstring>

ExecutionContext::ExecutionContext(byte* script, int32 scriptLength, bool pushOnly, int32 instructorPointer)
	:ScriptLength(scriptLength), IsPushOnly(pushOnly), InstructionPointer(instructorPointer), IsScriptHashCalculated(false)
{
	// Copy script

	this->Script = new unsigned char[scriptLength];
	memcpy(this->Script, script, scriptLength);
}

bool ExecutionContext::ReadUInt8(byte &ret)
{
	unsigned char data[1];

	if (Read(data, 1) != 1)
		return false;

	ret = (unsigned __int8)data[0];
	return true;
}

bool ExecutionContext::ReadUInt16(uint16 &ret)
{
	unsigned char data[2];

	if (Read(data, 2) != 2)
		return false;

	ret = (unsigned __int16)((int)data[0] | (int)data[1] << 8);
	return true;
}

bool ExecutionContext::ReadInt16(int16 &ret)
{
	unsigned char data[2];

	if (Read(data, 2) != 2)
		return false;

	ret = (__int16)((int)data[0] | (int)data[1] << 8);
	return true;
}

bool ExecutionContext::ReadUInt32(uint32 &ret)
{
	unsigned char data[4];

	if (Read(data, 4) != 4)
		return false;

	ret = (unsigned __int32)((int)data[0] | (int)data[1] << 8 | (int)data[2] << 16 | (int)data[3] << 24);
	return true;
}

bool ExecutionContext::ReadInt32(int32 &ret)
{
	unsigned char data[4];

	if (Read(data, 4) != 4)
		return false;

	ret = (__int32)((int)data[0] | (int)data[1] << 8 | (int)data[2] << 16 | (int)data[3] << 24);
	return true;
}

bool ExecutionContext::ReadInt64(int64 &ret)
{
	unsigned char data[8];

	if (Read(data, 8) != 8)
		return false;

	int a = ((__int32)((int)data[0] | (int)data[1] << 8 | (int)data[2] << 16 | (int)data[3] << 24));
	int b = ((__int32)((int)data[4] | (int)data[5] << 8 | (int)data[6] << 16 | (int)data[7] << 24));

	ret = (__int64)(b << 32 | a);
	return true;
}

bool ExecutionContext::ReadUInt64(uint64 &ret)
{
	unsigned char data[8];

	if (Read(data, 8) != 8)
		return false;

	unsigned int a = ((unsigned __int32)((int)data[0] | (int)data[1] << 8 | (int)data[2] << 16 | (int)data[3] << 24));
	unsigned int b = ((unsigned __int32)((int)data[4] | (int)data[5] << 8 | (int)data[6] << 16 | (int)data[7] << 24));

	ret = (unsigned __int64)(b << 32 | a);
	return true;
}

bool ExecutionContext::ReadVarBytes(int64 &ret, int64 max)
{
	unsigned __int8 fb = 0;

	if (!this->ReadUInt8(fb))
		return false;

	if (fb == 0xFD)
	{
		unsigned __int16 v = 0;

		if (!this->ReadUInt16(v))
			return false;

		ret = v;
	}
	else if (fb == 0xFE)
	{
		unsigned __int32 v = 0;

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

	delete(this->Script);
	this->Script = NULL;
}