#pragma once

#include "Types.h"

template <class T>
class Stack
{
private:

	int32 _size;
	int32 _itemsLength;
	T** _items;

	void EnsureCapacity(int32 min);
	void RealRemoveAt(int32 index);

public:

	// static const int32 SizeOf = sizeof(void*);

	inline int32 Count() const { return this->_size; }

	void SendTo(Stack<T>* stack, int32 count);

	void Drop(int32 index);
	void Clear();
	T* Pop();
	T* Pop(int32 index);

	void Push(T* item);
	void Insert(int32 index, T* item);
	T* Peek(int32 index) const;

	inline T* Top() const
	{
		if (this->_size == 0)
		{
			return NULL;
		}

		return this->_items[this->_size - 1];
	}

	inline T* Bottom() const
	{
		if (this->_size == 0)
		{
			return NULL;
		}

		return this->_items[0];
	}

	// Constructor & Destructor

	inline Stack() : _size(0), _itemsLength(4), _items(new T*[4]) { }

	inline ~Stack()
	{
		this->Clear();
	}
};