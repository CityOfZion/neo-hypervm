#include "ByteArrayStackItem.h"
#include <cstring>

ByteArrayStackItem::ByteArrayStackItem(byte* data, int32 size, bool copyPointer) :IStackItem(EStackItemType::ByteArray), PayloadLength(size)
{
	if (size > 0 && data != NULL)
	{
		if (copyPointer)
		{
			this->Payload = data;
		}
		else
		{
			this->Payload = new byte[size];
			memcpy(this->Payload, data, size);
		}
	}
	else
	{
		this->Payload = NULL;
	}
}

int32 ByteArrayStackItem::ReadByteArray(byte* output, int32 sourceIndex, int32 count)
{
	if (sourceIndex < 0)
	{
		return -1;
	}

	int32 l = count > this->PayloadLength - sourceIndex ? this->PayloadLength - sourceIndex : count;

	if (l > 0)
	{
		memcpy(output, &this->Payload[sourceIndex], l);
	}

	return l;
}

int32 ByteArrayStackItem::ReadByteArraySize()
{
	return this->PayloadLength;
}

ByteArrayStackItem::~ByteArrayStackItem()
{
	if (this->Payload == NULL)
		return;

	delete[](this->Payload);
	this->Payload = NULL;
}

bool ByteArrayStackItem::GetBoolean()
{
	for (int32 x = 0; x < this->PayloadLength; ++x)
		if (this->Payload[x] != 0x00)
			return true;

	return false;
}

BigInteger* ByteArrayStackItem::GetBigInteger()
{
	if (this->PayloadLength == 0)
		return new BigInteger(BigInteger::Zero);

	return new BigInteger(this->Payload, this->PayloadLength);
}

bool ByteArrayStackItem::GetInt32(int32 &ret)
{
	BigInteger* bi = this->GetBigInteger();
	if (bi == NULL) return false;

	bool bret = bi->ToInt32(ret);
	delete(bi);

	return bret;
}

bool ByteArrayStackItem::Equals(IStackItem* it)
{
	if (it == this) return true;

	switch (it->Type)
	{
	case EStackItemType::ByteArray:
	{
		ByteArrayStackItem* t = (ByteArrayStackItem*)it;
		if (t->PayloadLength != this->PayloadLength) return false;

		for (int x = t->PayloadLength - 1; x >= 0; x--)
			if (t->Payload[x] != this->Payload[x])
				return false;

		return true;
	}
	default:
	{
		int iz = it->ReadByteArraySize();

		if (iz < 0)
			return false;

		if (iz == 0)
			return this->PayloadLength == 0;

		byte* data = new byte[iz];
		iz = it->ReadByteArray(data, 0, iz);

		if (iz != this->PayloadLength)
		{
			delete[](data);
			return false;
		}

		for (int x = 0; x < iz; ++x)
			if (data[x] != this->Payload[x])
			{
				delete[](data);
				return false;
			}

		delete[](data);
		return true;
	}
	}
}

// Serialize

int32 ByteArrayStackItem::Serialize(byte* data, int32 length)
{
	if (this->PayloadLength > 0 && length > 0)
	{
		length = this->PayloadLength > length ? length : this->PayloadLength;
		memcpy(data, this->Payload, length);

		return length;
	}

	return 0;
}

int32 ByteArrayStackItem::GetSerializedSize()
{
	return this->PayloadLength;
}