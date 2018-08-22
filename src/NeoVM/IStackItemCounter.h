#pragma once

#include "Types.h"

class IStackItemCounter
{
private:

	int32 _items;
	int32 _maxItems;

public:

	inline void ItemCounterClean()
	{
		_items = 0;
	}

	inline bool ItemCounterInc()
	{
		return _maxItems >= ++_items;
	}

	inline bool ItemCounterInc(int32 count)
	{
		_items += count;

		return _maxItems >= _items;
	}

	inline void ItemCounterDec()
	{
		--_items;
	}

	inline IStackItemCounter(int32 maxItems) :_items(0), _maxItems(maxItems) {}
};