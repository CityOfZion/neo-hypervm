#include "BoolStackItem.h"
#include <cstring>

BoolStackItem::BoolStackItem(bool value) :IStackItem(EStackItemType::Bool), Value(value) { }
BoolStackItem::~BoolStackItem() { }

bool BoolStackItem::GetBoolean()
{
	return this->Value;
}

BigInteger * BoolStackItem::GetBigInteger()
{
	return new BigInteger(this->Value ? 1 : 0);
}

bool BoolStackItem::GetInt32(int &ret)
{
	ret = this->Value ? 1 : 0;
	return true;
}

IStackItem* BoolStackItem::Clone()
{
	return new BoolStackItem(Value);
}

int BoolStackItem::ReadByteArray(unsigned char * output, int sourceIndex, int count)
{
	if (sourceIndex != 0)
	{
		return -1;
	}

	if (count < 0)
	{
		return 0;
	}

	if (this->Value)
	{
		if (count == 0) return 0;

		output[0] = 0x01;

		return 1;
	}
	else
	{
		return 0;
	}
}

int BoolStackItem::ReadByteArraySize()
{
	return 1;
}

// Serialize

int BoolStackItem::Serialize(unsigned char * data, int length)
{
	if (length <= 0) return 0;

	data[0] = Value ? 0x01 : 0x00;
	return 1;
}

int BoolStackItem::GetSerializedSize()
{
	return 1;
}