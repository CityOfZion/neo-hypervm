#include "InteropStackItem.h"
#include <cstring>

InteropStackItem::InteropStackItem(unsigned char * data, int size) :IStackItem(EStackItemType::Interop)
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

bool InteropStackItem::GetInt32(int &ret)
{
	return false;
}

int InteropStackItem::ReadByteArray(unsigned char * output, int sourceIndex, int count)
{
	return -1;
}

int InteropStackItem::ReadByteArraySize()
{
	return -1;
}

IStackItem* InteropStackItem::Clone()
{
	return new InteropStackItem(this->Payload, this->PayloadLength);
}

// Serialize

int InteropStackItem::Serialize(unsigned char * data, int length)
{
	if (this->PayloadLength > 0 && length > 0)
	{
		length = this->PayloadLength > length ? length : this->PayloadLength;
		memcpy(data, this->Payload, length);

		return length;
	}
	return 0;
}

int InteropStackItem::GetSerializedSize()
{
	return this->PayloadLength;
}