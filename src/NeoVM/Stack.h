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

	const int32 SizeOf = sizeof(void*);

	inline int32 Count() { return this->_size; }

	void CopyTo(Stack<T>* stack, int32 count);
	void SendTo(Stack<T>* stack, int32 count);

	void Drop(int32 index);
	void Clear();
	T* Pop();
	T* Pop(int32 index);

	void Push(T* item);
	void Insert(int32 index, T* item);
	T* Peek(int32 index);

	T* Top();
	T* Bottom();

	inline Stack() : _size(0), _itemsLength(0), _items(NULL) { }
	~Stack();
};