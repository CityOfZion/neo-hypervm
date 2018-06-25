#include "BoolStackItem.h"

BoolStackItem::BoolStackItem(bool value) :IStackItem(EStackItemType::Bool), Value(value) { }
BoolStackItem::~BoolStackItem() { }

bool BoolStackItem::GetBoolean()
{
	return this->Value;
}

BigInteger* BoolStackItem::GetBigInteger()
{
	return new BigInteger(this->Value ? BigInteger::One : BigInteger::Zero);
}

bool BoolStackItem::GetInt32(int32 &ret)
{
	ret = this->Value ? 1 : 0;
	return true;
}

bool BoolStackItem::Equals(IStackItem* it)
{
	if (it == this) return true;

	switch (it->Type)
	{
	case EStackItemType::Bool:
	{
		BoolStackItem* t = (BoolStackItem*)it;
		return this->Value == t->Value;
	}
	default:
	{
		switch (it->ReadByteArraySize())
		{
		case 1:
		{
			byte* data = new byte[1];
			int iz = it->ReadByteArray(data, 0, 1);

			// Current true

			if (this->Value)
			{
				bool ret = (data[0] == 0x01);
				delete[](data);
				return ret;
			}

			// Current false

			delete[](data);
			return iz == 0;
		}
		case 0: return !this->Value;
		default: return false;
		}
	}
	}
}

int32 BoolStackItem::ReadByteArray(byte* output, int32 sourceIndex, int32 count)
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
	return this->Value ? 1 : 0;
}

// Serialize

int32 BoolStackItem::Serialize(byte* data, int32 length)
{
	if (length <= 0) return 0;

	data[0] = Value ? 0x01 : 0x00;
	return 1;
}

int32 BoolStackItem::GetSerializedSize()
{
	return 1;
}