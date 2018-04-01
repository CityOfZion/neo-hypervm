#include "MapStackItem.h"

MapStackItem::MapStackItem() :IStackItem(EStackItemType::Map)
{

}

MapStackItem::~MapStackItem()
{

}

int32 MapStackItem::Count()
{
	return -1;
}

bool MapStackItem::GetBoolean()
{
	return true;
}

BigInteger * MapStackItem::GetBigInteger()
{
	return NULL;
}

bool MapStackItem::GetInt32(int32 &ret)
{
	return false;
}

int32 MapStackItem::ReadByteArray(byte * output, int32 sourceIndex, int32 count)
{
	return -1;
}

int32 MapStackItem::ReadByteArraySize()
{
	return -1;
}

IStackItem* MapStackItem::Clone()
{
	// TODO: Fix
	return new MapStackItem();
}

void MapStackItem::Set(IStackItem* key, IStackItem* value)
{

}

// Serialize

int32 MapStackItem::Serialize(byte * data, int32 length)
{
	return 0;
}

int32 MapStackItem::GetSerializedSize()
{
	return 0;
}