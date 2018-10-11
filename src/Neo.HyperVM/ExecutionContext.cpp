#include "ExecutionContext.h"
#include "Crypto.h"

bool ExecutionContext::ReadUInt8(byte &ret)
{
	if (Read(this->_buffer, 1) != 1)
	{
		return false;
	}

	ret = this->_buffer[0];
	return true;
}

bool ExecutionContext::ReadUInt16(uint16 &ret)
{
	if (Read(this->_buffer, 2) != 2)
	{
		return false;
	}

	ret = (uint16)((int32)this->_buffer[0] | (int32)this->_buffer[1] << 8);
	return true;
}

bool ExecutionContext::ReadInt16(int16 &ret)
{
	if (Read(this->_buffer, 2) != 2)
	{
		return false;
	}

	ret = (int16)((int32)this->_buffer[0] | (int32)this->_buffer[1] << 8);
	return true;
}

bool ExecutionContext::ReadUInt32(uint32 &ret)
{
	if (Read(this->_buffer, 4) != 4)
	{
		return false;
	}

	ret = (uint32)
		(
		(int32)this->_buffer[0] | (int32)this->_buffer[1] << 8 |
		(int32)this->_buffer[2] << 16 | (int32)this->_buffer[3] << 24
		);

	return true;
}

bool ExecutionContext::ReadInt32(int32 &ret)
{
	if (Read(this->_buffer, 4) != 4)
	{
		return false;
	}

	ret = (int32)
		(
		(int32)this->_buffer[0] | (int32)this->_buffer[1] << 8 |
		(int32)this->_buffer[2] << 16 | (int32)this->_buffer[3] << 24
		);

	return true;
}

bool ExecutionContext::ReadInt64(int64 &ret)
{
	if (Read(this->_buffer, 8) != 8)
	{
		return false;
	}

	uint32 a = ((uint32)
		(
		(int32)this->_buffer[0] | (int32)this->_buffer[1] << 8 |
		(int32)this->_buffer[2] << 16 | (int32)this->_buffer[3] << 24)
		);

	uint32 b = ((uint32)
		(
		(int32)this->_buffer[4] | (int32)this->_buffer[5] << 8 |
		(int32)this->_buffer[6] << 16 | (int32)this->_buffer[7] << 24)
		);

	ret = (int64)((uint64)b << 32 | (uint64)a);
	return true;
}

bool ExecutionContext::ReadUInt64(uint64 &ret)
{
	if (Read(this->_buffer, 8) != 8)
	{
		return false;
	}

	uint32 a = ((uint32)
		(
		(int32)this->_buffer[0] | (int32)this->_buffer[1] << 8 |
		(int32)this->_buffer[2] << 16 | (int32)this->_buffer[3] << 24)
		);

	uint32 b = ((uint32)
		(
		(int32)this->_buffer[4] | (int32)this->_buffer[5] << 8 |
		(int32)this->_buffer[6] << 16 | (int32)this->_buffer[7] << 24)
		);

	ret = ((uint64)b << 32 | (uint64)a);
	return true;
}

bool ExecutionContext::ReadVarBytes(uint32 &ret, uint32 max)
{
	byte fb = 0;

	if (!this->ReadUInt8(fb))
	{
		return false;
	}

	if (fb == 0xFD)
	{
		uint16 v = 0;

		if (!this->ReadUInt16(v))
		{
			return false;
		}

		ret = v;
	}
	else if (fb == 0xFE)
	{
		uint32 v = 0;

		if (!this->ReadUInt32(v))
		{
			return false;
		}

		ret = v;
	}
	else if (fb == 0xFF)
	{
		uint64 v = 0;

		// Never is needed read more than an integer

		if (!this->ReadUInt64(v) || v > 0xFFFFFFFF)
		{
			return false;
		}

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

		int32 read = this->_scriptLength - this->_instructionIndex;
		read = read < length ? read : length;

		this->_instructionIndex += read;
		this->_instructionPointer += read;

		return read;
	}
	else
	{
		int32 read = 0;

		for (int32 x = 0; x < length && this->_instructionIndex < this->_scriptLength; ++x)
		{
			data[x] = this->_script->Content[this->_instructionIndex];

			++this->_instructionIndex;
			++this->_instructionPointer;
			++read;
		}

		return read;
	}
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