#include "BoolStackItem.h"

bool BoolStackItem::Equals(IStackItem* it)
{
	if (it == this) return true;

	switch (it->Type)
	{
	case EStackItemType::Bool:
	{
		auto t = (BoolStackItem*)it;
		return this->_value == t->_value;
	}
	default:
	{
		switch (it->ReadByteArraySize())
		{
		case 1:
		{
			byte* data = new byte[1];
			int32 iz = it->ReadByteArray(data, 0, 1);

			// Current true

			if (this->_value)
			{
				bool ret = (data[0] == 0x01);
				delete[](data);
				return ret;
			}

			// Current false

			delete[](data);
			return iz == 0;
		}
		case 0: return !this->_value;
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

	if (this->_value)
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