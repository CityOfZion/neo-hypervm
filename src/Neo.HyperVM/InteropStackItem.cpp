#include "InteropStackItem.h"

bool InteropStackItem::Equals(IStackItem* it)
{
	if (it == this) return true;

	// Different type

	if (it->Type != EStackItemType::Interop)
		return false;

	auto ii = (InteropStackItem*)it;

	// Check content of the raw data

	if (ii->_payloadLength != this->_payloadLength)
		return false;

	for (int32 x = this->_payloadLength - 1; x >= 0; x--)
	{
		if (ii->_payload[x] != this->_payload[x])
			return false;
	}

	return true;
}