#pragma once

#include "Types.h"

class IStackItemCounter
{
private:

	int32 _items;
	int32 _maxItems;

public:

	inline bool ItemCounterInc()
	{
		return _maxItems >= ++_items;
	}
	
	inline void ItemCounterDec()
	{
		--_items;
	}

	inline IStackItemCounter(int32 maxItems) :_items(0), _maxItems(maxItems) {}
};