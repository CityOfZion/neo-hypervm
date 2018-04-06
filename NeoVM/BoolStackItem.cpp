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

bool BoolStackItem::GetInt32(int32 &ret)
{
	ret = this->Value ? 1 : 0;
	return true;
}

IStackItem* BoolStackItem::Clone()
{
	return new BoolStackItem(Value);
}

bool BoolStackItem::Equals(IStackItem * it)
{
	if (it == this) return true;

	return this->Value == it->GetBoolean();
}

int32 BoolStackItem::ReadByteArray(byte * output, int32 sourceIndex, int32 count)
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

int32 BoolStackItem::ReadByteArraySize()
{
	return 1;
}

// Serialize

int32 BoolStackItem::Serialize(byte * data, int32 length)
{
	if (length <= 0) return 0;

	data[0] = Value ? 0x01 : 0x00;
	return 1;
}

int32 BoolStackItem::GetSerializedSize()
{
	return 1;
}