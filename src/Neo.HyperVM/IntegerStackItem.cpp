#include "IntegerStackItem.h"

bool IntegerStackItem::Equals(IStackItem* it)
{
	if (it == this) return true;

	switch (it->Type)
	{
	case EStackItemType::Integer:
	{
		auto t = (IntegerStackItem*)it;
		return this->_value.CompareTo(t->_value) == 0;
	}
	default:
	{
		int32 i1 = it->ReadByteArraySize();

		if (i1 < 0)
		{
			return false;
		}

		int32 i0 = this->ReadByteArraySize();
		byte* d0 = new byte[i0];
		byte* d1 = new byte[i1];

		i0 = this->ReadByteArray(d0, 0, i0);
		i1 = it->ReadByteArray(d1, 0, i1);

		if (i1 != i0)
		{
			delete[](d0);
			delete[](d1);
			return false;
		}

		bool ret = true;
		for (int32 x = 0; x < i0; ++x)
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