#include "ByteArrayStackItem.h"
#include <cstring>

ByteArrayStackItem::ByteArrayStackItem(unsigned char * data, int size, bool copyPointer) :IStackItem(EStackItemType::ByteArray), PayloadLength(size)
{
	if (size > 0 && data != NULL)
	{
		if (copyPointer)
		{
			this->Payload = data;
		}
		else
		{
			this->Payload = new unsigned char[size];
			memcpy(this->Payload, data, size);
		}
	}
	else
	{
		this->Payload = NULL;
	}
}

int ByteArrayStackItem::ReadByteArray(unsigned char * output, int sourceIndex, int count)
{
	if (sourceIndex < 0)
	{
		return -1;
	}

	int l = count > this->PayloadLength - sourceIndex ? this->PayloadLength - sourceIndex : count;

	if (l > 0)
	{
		memcpy(output, &this->Payload[sourceIndex], l);
	}

	return l;
}

int ByteArrayStackItem::ReadByteArraySize()
{
	return this->PayloadLength;
}

ByteArrayStackItem::~ByteArrayStackItem()
{
	if (this->Payload != NULL)
	{
		delete(this->Payload);
		this->Payload = NULL;
	}
}

bool ByteArrayStackItem::GetBoolean()
{
	if (this->PayloadLength == 1)
	{
		return this->Payload[0] != 0x00;
	}
	else
	{
		if (this->PayloadLength <= 0) return false;

		for (int x = 0; x < this->PayloadLength; x++)
			if (this->Payload[x] != 0x00)
				return true;

		return false;
	}
}

BigInteger * ByteArrayStackItem::GetBigInteger()
{
	if (this->PayloadLength == 0)
		return new BigInteger(0);

	return new BigInteger(this->Payload, this->PayloadLength);
}

bool ByteArrayStackItem::GetInt32(int &ret)
{
	BigInteger * bi = this->GetBigInteger();
	if (bi == NULL) return false;

	bool bret = bi->ToInt32(ret);
	delete(bi);

	return bret;
}

IStackItem* ByteArrayStackItem::Clone()
{
	return new ByteArrayStackItem(this->Payload, this->PayloadLength, false);
}

// Serialize

int ByteArrayStackItem::Serialize(unsigned char * data, int length)
{
	if (this->PayloadLength > 0 && length > 0)
	{
		length = this->PayloadLength > length ? length : this->PayloadLength;
		memcpy(data, this->Payload, length);

		return length;
	}
	return 0;
}

int ByteArrayStackItem::GetSerializedSize()
{
	return this->PayloadLength;
}