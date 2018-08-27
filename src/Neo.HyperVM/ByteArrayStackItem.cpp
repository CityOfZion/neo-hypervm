#include "ByteArrayStackItem.h"
#include <string.h>

ByteArrayStackItem::ByteArrayStackItem(volatile IStackItemCounter* &counter, byte* data, int32 size, bool copyPointer) :
	IStackItem(counter, EStackItemType::ByteArray),
	_payloadLength(size)
{
	if (size > 0 && data != NULL)
	{
		if (copyPointer)
		{
			this->_payload = data;
		}
		else
		{
			this->_payload = new byte[size];
			memcpy(this->_payload, data, size);
		}
	}
	else
	{
		this->_payload = NULL;
	}
}

int32 ByteArrayStackItem::ReadByteArray(byte* output, int32 sourceIndex, int32 count)
{
	if (sourceIndex < 0)
	{
		return -1;
	}

	int32 l = count > this->_payloadLength - sourceIndex ? this->_payloadLength - sourceIndex : count;

	if (l > 0)
	{
		memcpy(output, &this->_payload[sourceIndex], l);
	}

	return l;
}

bool ByteArrayStackItem::Equals(IStackItem* it)
{
	if (it == this) return true;

	switch (it->Type)
	{
	case EStackItemType::ByteArray:
	{
		auto t = (ByteArrayStackItem*)it;
		if (t->_payloadLength != this->_payloadLength) return false;

		for (int x = t->_payloadLength - 1; x >= 0; x--)
			if (t->_payload[x] != this->_payload[x])
				return false;

		return true;
	}
	default:
	{
		int iz = it->ReadByteArraySize();

		if (iz < 0)
			return false;

		if (iz == 0)
			return this->_payloadLength == 0;

		byte* data = new byte[iz];
		iz = it->ReadByteArray(data, 0, iz);

		if (iz != this->_payloadLength)
		{
			delete[](data);
			return false;
		}

		for (int x = 0; x < iz; ++x)
			if (data[x] != this->_payload[x])
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
	if (this->_payloadLength > 0 && length > 0)
	{
		length = this->_payloadLength > length ? length : this->_payloadLength;
		memcpy(data, this->_payload, length);

		return length;
	}

	return 0;
}