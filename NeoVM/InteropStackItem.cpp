#include "InteropStackItem.h"
#include <cstring>

InteropStackItem::InteropStackItem(byte * data, int32 size) :IStackItem(EStackItemType::Interop)
{
	this->Payload = new unsigned char[size];
	this->PayloadLength = size;

	memcpy(this->Payload, data, size);
}

InteropStackItem::~InteropStackItem()
{
	delete(this->Payload);
}

bool InteropStackItem::GetBoolean()
{
	return this->PayloadLength > 0;
}

BigInteger * InteropStackItem::GetBigInteger()
{
	return NULL;
}

bool InteropStackItem::GetInt32(int32 &ret)
{
	return false;
}

int32 InteropStackItem::ReadByteArray(byte * output, int32 sourceIndex, int32 count)
{
	return -1;
}

int32 InteropStackItem::ReadByteArraySize()
{
	return -1;
}

IStackItem* InteropStackItem::Clone()
{
	return new InteropStackItem(this->Payload, this->PayloadLength);
}

// Serialize

int32 InteropStackItem::Serialize(byte * data, int32 length)
{
	if (this->PayloadLength > 0 && length > 0)
	{
		length = this->PayloadLength > length ? length : this->PayloadLength;
		memcpy(data, this->Payload, length);

		return length;
	}
	return 0;
}

int32 InteropStackItem::GetSerializedSize()
{
	return this->PayloadLength;
}