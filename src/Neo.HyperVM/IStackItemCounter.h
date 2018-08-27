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
		this->_items = 0;
	}

	inline bool ItemCounterInc()
	{
		return this->_maxItems >= ++this->_items;
	}

	inline bool ItemCounterInc(int32 count)
	{
		this->_items += count;

		return this->_maxItems >= this->_items;
	}

	inline void ItemCounterDec()
	{
		--this->_items;
	}

	inline IStackItemCounter(int32 maxItems) :_items(0), _maxItems(maxItems) {}
};