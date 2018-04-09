#include "IntegerStackItem.h"

IntegerStackItem::IntegerStackItem(byte * data, int32 size) :IStackItem(EStackItemType::Integer),
Value(new BigInteger(data, size)) {}

IntegerStackItem::IntegerStackItem(int32 value) : IStackItem(EStackItemType::Integer),
Value(new BigInteger(value)) {}

IntegerStackItem::IntegerStackItem(BigInteger *value, bool copyPointer) : IStackItem(EStackItemType::Integer),
Value(copyPointer ? value : new BigInteger(value)) {}

IntegerStackItem::~IntegerStackItem()
{
	if (this->Value == NULL) return;

	delete(this->Value);
	this->Value = NULL;
}

bool IntegerStackItem::GetBoolean()
{
	return this->Value->CompareTo(BigInteger::Zero) != 0;
}

BigInteger* IntegerStackItem::GetBigInteger()
{
	return this->Value->Clone();
}

bool IntegerStackItem::GetInt32(int32 &ret)
{
	return this->Value->ToInt32(ret);
}

int32 IntegerStackItem::ReadByteArray(byte * output, int32 sourceIndex, int32 count)
{
	if (sourceIndex != 0)
	{
		return -1;
	}

	return this->Value->ToByteArray(output, count);
}

int32 IntegerStackItem::ReadByteArraySize()
{
	return this->Value->ToByteArraySize();
}

IStackItem* IntegerStackItem::Clone()
{
	return new IntegerStackItem(this->Value, false);
}

bool IntegerStackItem::Equals(IStackItem * it)
{
	if (it == this) return true;

	switch (it->Type)
	{
	case EStackItemType::Integer:
	{
		IntegerStackItem* t = (IntegerStackItem*)it;
		return this->Value->CompareTo(t->Value) == 0;
	}
	default:
	{
		int i1 = it->ReadByteArraySize();

		if (i1 < 0)
		{
			return false;
		}

		int i0 = this->ReadByteArraySize();
		byte * d0 = new byte[i0];
		byte * d1 = new byte[i1];

		i0 = this->ReadByteArray(d0, 0, i0);
		i1 = it->ReadByteArray(d1, 0, i1);

		if (i1 != i0)
		{
			delete[](d0);
			delete[](d1);
			return false;
		}

		bool ret = true;
		for (int x = 0; x < i0; x++)
			if (d0[x] != d1[x])
			{
				ret = false;
				break;
			}

		delete[](d0);
		delete[](d1);
		return ret;
	}
	}
}

// Serialize

int32 IntegerStackItem::Serialize(byte * data, int32 length)
{
	return this->Value->ToByteArray(data, length);
}

int32 IntegerStackItem::GetSerializedSize()
{
	return this->Value->ToByteArraySize();
}