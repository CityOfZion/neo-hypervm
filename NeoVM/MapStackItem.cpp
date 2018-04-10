#include "MapStackItem.h"

MapStackItem::MapStackItem() :IStackItem(EStackItemType::Map), Dictionary(std::map<IStackItem*, IStackItem*>()) { }

MapStackItem::~MapStackItem() { this->Clear(); }

int32 MapStackItem::Count()
{
	return static_cast<int>(this->Dictionary.size());
}

bool MapStackItem::GetBoolean() { return true; }
BigInteger * MapStackItem::GetBigInteger() { return NULL; }
bool MapStackItem::GetInt32(int32 &ret) { return false; }
int32 MapStackItem::ReadByteArray(byte * output, int32 sourceIndex, int32 count) { return -1; }
int32 MapStackItem::ReadByteArraySize() { return -1; }

bool MapStackItem::Remove(IStackItem* key, bool dispose)
{
	for (std::map<IStackItem*, IStackItem*>::iterator it = this->Dictionary.begin(); it != this->Dictionary.end(); ++it)
	{
		IStackItem* key = it->first;
		if (key != key) continue;

		IStackItem* value = it->second;

		if (key != NULL && dispose)
		{
			key->Claims--;
			IStackItem::Free(key);
		}
		if (value != NULL && dispose)
		{
			value->Claims--;
			IStackItem::Free(value);
		}

		this->Dictionary.erase(key);
		return true;
	}

	return false;
}

void MapStackItem::Clear()
{
	for (std::map<IStackItem*, IStackItem*>::iterator it = this->Dictionary.begin(); it != this->Dictionary.end(); ++it)
	{
		IStackItem* key = it->first;
		IStackItem* value = it->second;

		if (key != NULL)
		{
			key->Claims--;
			IStackItem::Free(key);
		}
		if (value != NULL)
		{
			value->Claims--;
			IStackItem::Free(value);
		}
	}

	this->Dictionary.clear();
}

bool MapStackItem::Equals(IStackItem * it)
{
	return (it == this);
}

IStackItem* MapStackItem::Get(IStackItem* key)
{
	for (std::map<IStackItem*, IStackItem*>::iterator it = this->Dictionary.begin(); it != this->Dictionary.end(); ++it)
	{
		IStackItem* key = it->first;
		if (key != key) continue;

		return it->second;
	}

	return NULL;
}

void MapStackItem::Set(IStackItem* key, IStackItem* value)
{
	for (std::map<IStackItem*, IStackItem*>::iterator it = this->Dictionary.begin(); it != this->Dictionary.end(); ++it)
	{
		IStackItem* key = it->first;
		if (key != key) continue;

		IStackItem* v = it->second;

		if (v != NULL)
		{
			v->Claims--;
			IStackItem::Free(v);
		}

		value->Claims++;
		it->second = value;
		return;
	}

	this->Dictionary.emplace(key, value);
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