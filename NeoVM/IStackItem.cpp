#include "IStackItem.h"

// Constructor

IStackItem::IStackItem(EStackItemType type) :Type(type), Claims(0) { }

// Methods

void IStackItem::Free(IStackItem* &item)
{
	if (item == NULL) return;

	if (item->Claims <= 1)
	{
		delete(item);
		item = NULL;
	}
	else
	{
		item->Claims--;
	}
}