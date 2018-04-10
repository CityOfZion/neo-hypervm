#include "ExecutionEngine.h"
#include "ByteArrayStackItem.h"
#include "IntegerStackItem.h"
#include "BoolStackItem.h"
#include "MapStackItem.h"
#include "ArrayStackItem.h"
#include "Crypto.h"

// Getters

ExecutionContextStack* ExecutionEngine::GetInvocationStack() { return this->InvocationStack; }
StackItems* ExecutionEngine::GetEvaluationStack() { return this->EvaluationStack; }
StackItems* ExecutionEngine::GetAltStack() { return this->AltStack; }

// Setters

void ExecutionEngine::SetLogCallback(OnStepIntoCallback logCallback) { this->Log = logCallback; }

void ExecutionEngine::Clean(uint32 iteration)
{
	this->Iteration = iteration;
	this->State = EVMState::NONE;

	this->InvocationStack->Clear();
	this->EvaluationStack->Clear();
	this->AltStack->Clear();
}

// Constructor

ExecutionEngine::ExecutionEngine
(
	InvokeInteropCallback invokeInterop, LoadScriptCallback loadScript, GetMessageCallback getMessage
) :
	Iteration(0),
	State(EVMState::NONE),

	OnGetMessage(getMessage),
	OnLoadScript(loadScript),
	OnInvokeInterop(invokeInterop),

	AltStack(new StackItems()),
	EvaluationStack(new StackItems()),
	InvocationStack(new ExecutionContextStack())
{ }

void ExecutionEngine::LoadScript(byte * script, int32 scriptLength)
{
	this->InvocationStack->Push(new ExecutionContext(script, scriptLength, false, 0));
}

void ExecutionEngine::LoadPushOnlyScript(byte * script, int32 scriptLength)
{
	this->InvocationStack->Push(new ExecutionContext(script, scriptLength, true, 0));
}

ExecutionContext* ExecutionEngine::GetCurrentContext()
{
	return this->InvocationStack->Peek(0);
}

ExecutionContext* ExecutionEngine::GetCallingContext()
{
	return this->InvocationStack->Count() > 1 ? this->InvocationStack->Peek(1) : NULL;
}

ExecutionContext* ExecutionEngine::GetEntryContext()
{
	return this->InvocationStack->Peek(this->InvocationStack->Count() - 1);
}

byte ExecutionEngine::GetState()
{
	return this->State;
}

EVMState ExecutionEngine::Execute()
{
	do
	{
		this->StepInto();
	} while (this->State == EVMState::NONE);

	return this->State;
}

void ExecutionEngine::StepOut()
{
	int32 c = this->InvocationStack->Count();

	while (this->State == EVMState::NONE && this->InvocationStack->Count() >= c)
		this->StepInto();
}

void ExecutionEngine::StepOver()
{
	if (this->State != EVMState::NONE) return;

	int32 c = this->InvocationStack->Count();

	do
	{
		this->StepInto();
	} while (this->State == EVMState::NONE && this->InvocationStack->Count() > c);
}

void ExecutionEngine::StepInto()
{
	if (this->State != EVMState::NONE)
	{
		return;
	}

	ExecutionContext * context = this->InvocationStack->TryPeek(0);

	if (context == NULL)
	{
		this->State = EVMState::HALT;
		return;
	}

	if (this->Log != NULL)
	{
		this->Log(context);
	}

	EVMOpCode opcode = context->ReadNextInstruction();

	// Check PushOnly

	if (context->IsPushOnly && opcode > EVMOpCode::PUSH16 && opcode != EVMOpCode::RET)
	{
		this->State = EVMState::FAULT;
		return;
	}

ExecuteOpCode:

	// Check PushBytes

	if (opcode >= EVMOpCode::PUSHBYTES1 && opcode <= EVMOpCode::PUSHBYTES75)
	{
		byte length = (byte)opcode;
		byte *data = new byte[length];

		if (context->Read(data, length) != length)
		{
			delete[](data);
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new ByteArrayStackItem(data, length, true));
		return;
	}

	// Check Push

	if (opcode >= EVMOpCode::PUSH1 && opcode <= EVMOpCode::PUSH16)
	{
		this->EvaluationStack->Push(new IntegerStackItem((opcode - EVMOpCode::PUSH1) + 1));
		return;
	}

	// Execute opcode

	switch (opcode)
	{

		// Push value

	case EVMOpCode::PUSH0:
	{
		this->EvaluationStack->Push(new ByteArrayStackItem(NULL, 0, false));
		return;
	}
	case EVMOpCode::PUSHDATA1:
	{
		byte length = 0;
		if (!context->ReadUInt8(length))
		{
			this->State = EVMState::FAULT;
			return;
		}

		byte *data = new byte[length];
		if (context->Read(data, length) != length)
		{
			delete[](data);
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new ByteArrayStackItem(data, length, true));
		return;
	}
	case EVMOpCode::PUSHDATA2:
	{
		uint16 length = 0;
		if (!context->ReadUInt16(length))
		{
			this->State = EVMState::FAULT;
			return;
		}

		byte *data = new byte[length];
		if (context->Read(data, length) != length)
		{
			delete[](data);
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new ByteArrayStackItem(data, length, true));
		return;
	}
	case EVMOpCode::PUSHDATA4:
	{
		int32 length = 0;
		if (!context->ReadInt32(length) || length < 0)
		{
			this->State = EVMState::FAULT;
			return;
		}

		byte *data = new byte[length];
		if (context->Read(data, length) != length)
		{
			delete[](data);
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new ByteArrayStackItem(data, length, true));
		return;
	}
	case EVMOpCode::PUSHM1:
	{
		this->EvaluationStack->Push(new IntegerStackItem(new BigInteger(BigInteger::MinusOne), true));
		return;
	}

	// Control

	case EVMOpCode::NOP: return;
	case EVMOpCode::JMP:
	{
		int16 offset = 0;
		if (!context->ReadInt16(offset))
		{
			this->State = EVMState::FAULT;
			return;
		}

		offset = context->InstructionPointer + offset - 3;

		if (offset < 0 || offset > context->ScriptLength)
		{
			this->State = EVMState::FAULT;
			return;
		}

		context->InstructionPointer = offset;
		return;
	}
	case EVMOpCode::JMPIF:
	case EVMOpCode::JMPIFNOT:
	{
		int16 offset = 0;
		if (!context->ReadInt16(offset))
		{
			this->State = EVMState::FAULT;
			return;
		}

		offset = context->InstructionPointer + offset - 3;

		if (offset < 0 || offset > context->ScriptLength || this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *bitem = this->EvaluationStack->Pop();

		if (opcode == EVMOpCode::JMPIFNOT)
		{
			if (!bitem->GetBoolean())
				context->InstructionPointer = offset;
		}
		else
		{
			if (bitem->GetBoolean())
				context->InstructionPointer = offset;
		}

		IStackItem::Free(bitem);
		return;
	}
	case EVMOpCode::CALL:
	{
		if (context == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		ExecutionContext * clone = context->Clone();

		this->InvocationStack->Push(clone);
		context->InstructionPointer += 2;

		// Jump

		opcode = EVMOpCode::JMP;
		context = clone;
		goto ExecuteOpCode;
	}
	case EVMOpCode::RET:
	{
		if (context != NULL)
		{
			context = NULL;
			this->InvocationStack->Drop();
		}

		if (this->InvocationStack->Count() == 0)
			this->State = EVMState::HALT;

		return;
	}
	case EVMOpCode::APPCALL:
	case EVMOpCode::TAILCALL:
	{
		if (this->OnLoadScript == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		byte script_hash[20];
		if (context->Read(script_hash, 20) != 20)
		{
			this->State = EVMState::FAULT;
			return;
		}

		bool isEmpty = true;
		for (int32 x = 0; x < 20; x++)
			if (script_hash[x] != 0x00) { isEmpty = false; break; }

		if (isEmpty)
		{
			if (this->EvaluationStack->Count() < 1)
			{
				this->State = EVMState::FAULT;
				return;
			}

			IStackItem* item = this->EvaluationStack->Pop();
			if (item->ReadByteArray(&script_hash[0], 0, 20) != 20)
			{
				IStackItem::Free(item);
				this->State = EVMState::FAULT;
				return;
			}

			IStackItem::Free(item);
		}

		if (opcode == EVMOpCode::TAILCALL)
		{
			this->InvocationStack->Drop();
		}

		if (this->OnLoadScript(script_hash) != 0x01)
		{
			this->State = EVMState::FAULT;
			return;
		}

		return;
	}
	case EVMOpCode::SYSCALL:
	{
		uint32 length = 0;
		if (this->OnInvokeInterop == NULL || !context->ReadVarBytes(length, 252) || length == 0)
		{
			this->State = EVMState::FAULT;
			return;
		}

		byte *data = new byte[length + 1];
		if (context->Read((byte*)data, length) != (int32)length)
		{
			delete[](data);
			this->State = EVMState::FAULT;
			return;
		}

		data[length] = 0x00;

		if (this->OnInvokeInterop(data, (int32)length) != 0x01)
			this->State = EVMState::FAULT;

		delete[](data);
		return;
	}

	// Stack ops

	case EVMOpCode::DUPFROMALTSTACK:
	{
		if (this->AltStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(this->AltStack->Peek(0));
		return;
	}
	case EVMOpCode::TOALTSTACK:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->AltStack->Push(this->EvaluationStack->Pop());
		return;
	}
	case EVMOpCode::FROMALTSTACK:
	{
		if (this->AltStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(this->AltStack->Pop());
		return;
	}
	case EVMOpCode::XDROP:
	{
		int32 ic = this->EvaluationStack->Count();
		if (ic < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem * it = this->EvaluationStack->Pop();

		int32 n = 0;
		if (!it->GetInt32(n) || n < 0 || n >= ic - 1)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		it = this->EvaluationStack->Remove(n);
		IStackItem::Free(it);
		return;
	}
	case EVMOpCode::XSWAP:
	{
		int32 ic = this->EvaluationStack->Count();
		if (ic < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 n = 0;
		IStackItem * it = this->EvaluationStack->Pop();

		if (!it->GetInt32(n) || n < 0 || n >= ic - 1)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		if (n == 0) return;

		IStackItem* xn = this->EvaluationStack->Peek(n);

		it = this->EvaluationStack->Remove(0);
		this->EvaluationStack->Insert(0, xn);
		this->EvaluationStack->Remove(n);
		this->EvaluationStack->Insert(n, it);
		return;
	}
	case EVMOpCode::XTUCK:
	{
		int32 ic = this->EvaluationStack->Count();
		if (ic < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 n = 0;
		IStackItem * it = this->EvaluationStack->Pop();

		if (!it->GetInt32(n) || n <= 0 || n > ic - 1)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		this->EvaluationStack->Insert(n, this->EvaluationStack->Peek(0));
		return;
	}
	case EVMOpCode::DEPTH:
	{
		this->EvaluationStack->Push(new IntegerStackItem(this->EvaluationStack->Count()));
		return;
	}
	case EVMOpCode::DROP:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *it = this->EvaluationStack->Pop();
		IStackItem::Free(it);
		return;
	}
	case EVMOpCode::DUP:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(this->EvaluationStack->Peek(0));
		return;
	}
	case EVMOpCode::NIP:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *x2 = this->EvaluationStack->Pop();
		IStackItem *x1 = this->EvaluationStack->Pop();
		IStackItem::Free(x1);

		this->EvaluationStack->Push(x2);
		return;
	}
	case EVMOpCode::OVER:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *x2 = this->EvaluationStack->Pop();
		IStackItem *x1 = this->EvaluationStack->Peek(0);

		this->EvaluationStack->Push(x2);
		this->EvaluationStack->Push(x1);
		return;
	}
	case EVMOpCode::PICK:
	{
		int32 ic = this->EvaluationStack->Count();
		if (ic < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 n = 0;
		IStackItem * it = this->EvaluationStack->Pop();

		if (!it->GetInt32(n) || n < 0)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);

		if (n >= ic - 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(this->EvaluationStack->Peek(n));
		return;
	}
	case EVMOpCode::ROLL:
	{
		int32 ic = this->EvaluationStack->Count();
		if (ic < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 n = 0;
		IStackItem * it = this->EvaluationStack->Pop();

		if (!it->GetInt32(n) || n < 0)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);

		if (n >= ic - 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		if (n == 0) return;

		this->EvaluationStack->Push(this->EvaluationStack->Remove(n));
		return;
	}
	case EVMOpCode::ROT:
	{
		if (this->EvaluationStack->Count() < 3)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *x3 = this->EvaluationStack->Pop();
		IStackItem *x2 = this->EvaluationStack->Pop();
		IStackItem *x1 = this->EvaluationStack->Pop();

		this->EvaluationStack->Push(x2);
		this->EvaluationStack->Push(x3);
		this->EvaluationStack->Push(x1);
		return;
	}
	case EVMOpCode::SWAP:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *x2 = this->EvaluationStack->Pop();
		IStackItem *x1 = this->EvaluationStack->Pop();

		this->EvaluationStack->Push(x2);
		this->EvaluationStack->Push(x1);
		return;
	}
	case EVMOpCode::TUCK:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *x2 = this->EvaluationStack->Pop();
		IStackItem *x1 = this->EvaluationStack->Pop();

		this->EvaluationStack->Push(x2);
		this->EvaluationStack->Push(x1);
		this->EvaluationStack->Push(x1);
		return;
	}
	case EVMOpCode::CAT:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem * x2 = this->EvaluationStack->Pop();
		IStackItem * x1 = this->EvaluationStack->Pop();
		int32 size2 = x2->ReadByteArraySize();
		int32 size1 = x1->ReadByteArraySize();

		if (size2 < 0 || size1 < 0)
		{
			IStackItem::Free(x2);
			IStackItem::Free(x1);

			this->State = EVMState::FAULT;
			return;
		}

		byte * data = new byte[size2 + size1];
		x1->ReadByteArray(&data[0], 0, size1);
		x2->ReadByteArray(&data[size1], 0, size2);

		IStackItem::Free(x2);
		IStackItem::Free(x1);

		this->EvaluationStack->Push(new ByteArrayStackItem(data, size1 + size2, true));
		return;
	}
	case EVMOpCode::SUBSTR:
	{
		if (this->EvaluationStack->Count() < 3)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 count = 0;
		IStackItem * it = this->EvaluationStack->Pop();

		if (!it->GetInt32(count) || count < 0)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		it = this->EvaluationStack->Pop();
		int32 index = 0;

		if (!it->GetInt32(index) || index < 0)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		it = this->EvaluationStack->Pop();

		byte * data = new byte[count];
		if (it->ReadByteArray(&data[0], index, count) != count)
		{
			delete[]data;
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		this->EvaluationStack->Push(new ByteArrayStackItem(data, count, true));
		return;
	}
	case EVMOpCode::LEFT:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 count = 0;
		IStackItem * it = this->EvaluationStack->Pop();

		if (!it->GetInt32(count) || count < 0)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		it = this->EvaluationStack->Pop();

		byte * data = new byte[count];
		if (it->ReadByteArray(&data[0], 0, count) != count)
		{
			delete[]data;
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		this->EvaluationStack->Push(new ByteArrayStackItem(data, count, true));
		return;
	}
	case EVMOpCode::RIGHT:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 count = 0;
		IStackItem * it = this->EvaluationStack->Pop();

		if (!it->GetInt32(count) || count < 0)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		it = this->EvaluationStack->Pop();

		byte * data = new byte[count];
		if (it->ReadByteArray(&data[0], it->ReadByteArraySize() - count, count) != count)
		{
			delete[]data;
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		this->EvaluationStack->Push(new ByteArrayStackItem(data, count, true));
		return;
	}
	case EVMOpCode::SIZE:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem * item = this->EvaluationStack->Pop();
		int32 size = item->ReadByteArraySize();
		IStackItem::Free(item);

		if (size < 0)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(size));
		return;
	}

	// Bitwise logic

	case EVMOpCode::INVERT:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = this->EvaluationStack->Pop();
		BigInteger* bi = item->GetBigInteger();
		IStackItem::Free(item);

		if (bi == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = bi->Invert();
		delete(bi);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::AND:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = i1->And(i2);

		delete(i2);
		delete(i1);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::OR:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = i1->Or(i2);

		delete(i2);
		delete(i1);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::XOR:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = i1->Xor(i2);

		delete(i2);
		delete(i1);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::EQUAL:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		this->EvaluationStack->Push(new BoolStackItem(x1->Equals(x2)));

		IStackItem::Free(x2);
		IStackItem::Free(x1);
		return;
	}

	// Numeric

	case EVMOpCode::INC:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = this->EvaluationStack->Pop();
		BigInteger* bi = item->GetBigInteger();
		IStackItem::Free(item);

		if (bi == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *add = new BigInteger(BigInteger::One);
		BigInteger *ret = bi->Add(add);
		delete(add);
		delete(bi);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::DEC:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = this->EvaluationStack->Pop();
		BigInteger* bi = item->GetBigInteger();
		IStackItem::Free(item);

		if (bi == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *add = new BigInteger(BigInteger::One);
		BigInteger *ret = bi->Sub(add);
		delete(add);
		delete(bi);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::SIGN:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = this->EvaluationStack->Pop();
		BigInteger* bi = item->GetBigInteger();
		IStackItem::Free(item);

		if (bi == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int ret = bi->GetSign();
		delete(bi);

		this->EvaluationStack->Push(new IntegerStackItem(ret));
		return;
	}
	case EVMOpCode::NEGATE:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = this->EvaluationStack->Pop();
		BigInteger* bi = item->GetBigInteger();
		IStackItem::Free(item);

		if (bi == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = bi->Negate();
		delete(bi);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::ABS:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = this->EvaluationStack->Pop();
		BigInteger* bi = item->GetBigInteger();
		IStackItem::Free(item);

		if (bi == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = bi->Abs();
		delete(bi);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::NOT:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = this->EvaluationStack->Pop();
		BoolStackItem *bnew = new BoolStackItem(!item->GetBoolean());
		IStackItem::Free(item);

		this->EvaluationStack->Push(bnew);
		return;
	}
	case EVMOpCode::NZ:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x = this->EvaluationStack->Pop();
		BigInteger *i = x->GetBigInteger();
		IStackItem::Free(x);

		if (i == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret = new BoolStackItem(i->CompareTo(BigInteger::Zero) != 0);
		delete(i);

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::ADD:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* i2 = this->EvaluationStack->Pop();
		IStackItem* i1 = this->EvaluationStack->Pop();
		BigInteger* x2 = i2->GetBigInteger();
		BigInteger* x1 = i1->GetBigInteger();
		IStackItem::Free(i2);
		IStackItem::Free(i1);

		if (x2 == NULL || x1 == NULL)
		{
			if (x2 != NULL) delete(x2);
			if (x1 != NULL) delete(x1);

			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = x1->Add(x2);
		delete(x2);
		delete(x1);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::SUB:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* i2 = this->EvaluationStack->Pop();
		IStackItem* i1 = this->EvaluationStack->Pop();
		BigInteger* x2 = i2->GetBigInteger();
		BigInteger* x1 = i1->GetBigInteger();
		IStackItem::Free(i2);
		IStackItem::Free(i1);

		if (x2 == NULL || x1 == NULL)
		{
			if (x2 != NULL) delete(x2);
			if (x1 != NULL) delete(x1);

			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = x1->Sub(x2);
		delete(x2);
		delete(x1);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::MUL:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* i2 = this->EvaluationStack->Pop();
		IStackItem* i1 = this->EvaluationStack->Pop();
		BigInteger* x2 = i2->GetBigInteger();
		BigInteger* x1 = i1->GetBigInteger();
		IStackItem::Free(i2);
		IStackItem::Free(i1);

		if (x2 == NULL || x1 == NULL)
		{
			if (x2 != NULL) delete(x2);
			if (x1 != NULL) delete(x1);

			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = x1->Mul(x2);
		delete(x2);
		delete(x1);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::DIV:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* i2 = this->EvaluationStack->Pop();
		IStackItem* i1 = this->EvaluationStack->Pop();
		BigInteger* x2 = i2->GetBigInteger();
		BigInteger* x1 = i1->GetBigInteger();
		IStackItem::Free(i2);
		IStackItem::Free(i1);

		if (x2 == NULL || x1 == NULL)
		{
			if (x2 != NULL) delete(x2);
			if (x1 != NULL) delete(x1);

			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = x1->Div(x2);
		delete(x2);
		delete(x1);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::MOD:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* i2 = this->EvaluationStack->Pop();
		IStackItem* i1 = this->EvaluationStack->Pop();
		BigInteger* x2 = i2->GetBigInteger();
		BigInteger* x1 = i1->GetBigInteger();
		IStackItem::Free(i2);
		IStackItem::Free(i1);

		if (x2 == NULL || x1 == NULL)
		{
			if (x2 != NULL) delete(x2);
			if (x1 != NULL) delete(x1);

			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = x1->Mod(x2);
		delete(x2);
		delete(x1);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::SHL:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* n = this->EvaluationStack->Pop();
		IStackItem* x = this->EvaluationStack->Pop();

		BigInteger *in = n->GetBigInteger();
		BigInteger *ix = x->GetBigInteger();

		IStackItem::Free(n);
		IStackItem::Free(x);

		int32 ishift;
		if (in == NULL || ix == NULL || !in->ToInt32(ishift) || (ishift > MAX_SHL_SHR || ishift < MIN_SHL_SHR))
		{
			if (ix != NULL) delete(ix);
			if (in != NULL) delete(in);

			this->State = EVMState::FAULT;
			return;
		}

		delete(in);
		BigInteger *ret = ix->Shl(ishift);
		delete(ix);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::SHR:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* n = this->EvaluationStack->Pop();
		IStackItem* x = this->EvaluationStack->Pop();

		BigInteger *in = n->GetBigInteger();
		BigInteger *ix = x->GetBigInteger();

		IStackItem::Free(n);
		IStackItem::Free(x);

		int32 ishift;
		if (in == NULL || ix == NULL || !in->ToInt32(ishift) || (ishift > MAX_SHL_SHR || ishift < MIN_SHL_SHR))
		{
			if (ix != NULL) delete(ix);
			if (in != NULL) delete(in);

			this->State = EVMState::FAULT;
			return;
		}

		delete(in);
		BigInteger *ret = ix->Shr(ishift);
		delete(ix);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::BOOLAND:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BoolStackItem * ret = new BoolStackItem(x1->GetBoolean() && x2->GetBoolean());
		IStackItem::Free(x2);
		IStackItem::Free(x1);

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::BOOLOR:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BoolStackItem * ret = new BoolStackItem(x1->GetBoolean() || x2->GetBoolean());
		IStackItem::Free(x2);
		IStackItem::Free(x1);

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::NUMEQUAL:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret = new BoolStackItem(i1->CompareTo(i2) == 0);

		delete(i2);
		delete(i1);

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::NUMNOTEQUAL:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret = new BoolStackItem(i1->CompareTo(i2) != 0);

		delete(i2);
		delete(i1);

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::LT:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret = new BoolStackItem(i1->CompareTo(i2) < 0);

		delete(i1);
		delete(i2);

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::GT:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret = new BoolStackItem(i1->CompareTo(i2) > 0);

		delete(i2);
		delete(i1);

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::LTE:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret = new BoolStackItem(i1->CompareTo(i2) <= 0);

		delete(i2);
		delete(i1);

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::GTE:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret = new BoolStackItem(i1->CompareTo(i2) >= 0);

		delete(i2);
		delete(i1);

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::MIN:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret;
		if (i1->CompareTo(i2) >= 0)
		{
			delete(i1);
			ret = new IntegerStackItem(i2, true);
		}
		else
		{
			delete(i2);
			ret = new IntegerStackItem(i1, true);
		}

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::MAX:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret;
		if (i1->CompareTo(i2) >= 0)
		{
			delete(i2);
			ret = new IntegerStackItem(i1, true);
		}
		else
		{
			delete(i1);
			ret = new IntegerStackItem(i2, true);
		}

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::WITHIN:
	{
		if (this->EvaluationStack->Count() < 3)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* b = this->EvaluationStack->Pop();
		IStackItem* a = this->EvaluationStack->Pop();
		IStackItem* x = this->EvaluationStack->Pop();

		BigInteger *ib = b->GetBigInteger();
		BigInteger *ia = a->GetBigInteger();
		BigInteger *ix = x->GetBigInteger();

		IStackItem::Free(b);
		IStackItem::Free(a);
		IStackItem::Free(x);

		if (ib == NULL || ia == NULL || ix == NULL)
		{
			if (ib != NULL) delete(ib);
			if (ia != NULL) delete(ia);
			if (ix != NULL) delete(ix);

			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new BoolStackItem(ia->CompareTo(ix) <= 0 && ix->CompareTo(ib) < 0));

		delete(ib);
		delete(ia);
		delete(ix);
		return;
	}

	// Crypto

	case EVMOpCode::SHA1:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();
		int32 size = item->ReadByteArraySize();

		if (size < 0)
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}

		byte* data = new byte[size];
		size = item->ReadByteArray(data, 0, size);
		IStackItem::Free(item);

		if (size < 0)
		{
			delete[]data;
			this->State = EVMState::FAULT;
			return;
		}

		byte *hash = new byte[Crypto::SHA1_LENGTH];
		Crypto::ComputeSHA1(data, size, hash);
		delete[]data;

		this->EvaluationStack->Push(new ByteArrayStackItem(hash, Crypto::SHA1_LENGTH, true));
		return;
	}
	case EVMOpCode::SHA256:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();
		int32 size = item->ReadByteArraySize();

		if (size < 0)
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}

		byte* data = new byte[size];
		size = item->ReadByteArray(data, 0, size);
		IStackItem::Free(item);

		if (size < 0)
		{
			delete[]data;
			this->State = EVMState::FAULT;
			return;
		}

		byte *hash = new byte[Crypto::SHA256_LENGTH];
		Crypto::ComputeSHA256(data, size, hash);
		delete[]data;

		this->EvaluationStack->Push(new ByteArrayStackItem(hash, Crypto::SHA256_LENGTH, true));
		return;
	}
	case EVMOpCode::HASH160:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();
		int32 size = item->ReadByteArraySize();

		if (size < 0)
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}

		byte* data = new byte[size];
		size = item->ReadByteArray(data, 0, size);
		IStackItem::Free(item);

		if (size < 0)
		{
			delete[]data;
			this->State = EVMState::FAULT;
			return;
		}

		byte *hash = new byte[Crypto::HASH160_LENGTH];
		Crypto::ComputeHash160(data, size, hash);
		delete[]data;

		this->EvaluationStack->Push(new ByteArrayStackItem(hash, Crypto::HASH160_LENGTH, true));
		return;
	}
	case EVMOpCode::HASH256:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();
		int32 size = item->ReadByteArraySize();

		if (size < 0)
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}

		byte* data = new byte[size];
		size = item->ReadByteArray(data, 0, size);
		IStackItem::Free(item);

		if (size < 0)
		{
			delete[]data;
			this->State = EVMState::FAULT;
			return;
		}

		byte *hash = new byte[Crypto::HASH256_LENGTH];
		Crypto::ComputeHash256(data, size, hash);
		delete[]data;

		this->EvaluationStack->Push(new ByteArrayStackItem(hash, Crypto::HASH256_LENGTH, true));
		return;
	}
	case EVMOpCode::CHECKSIG:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* ipubKey = this->EvaluationStack->Pop();
		IStackItem* isignature = this->EvaluationStack->Pop();

		int pubKeySize = ipubKey->ReadByteArraySize();
		int signatureSize = isignature->ReadByteArraySize();

		if (this->OnGetMessage == NULL || pubKeySize < 33 || signatureSize < 32)
		{
			IStackItem::Free(ipubKey);
			IStackItem::Free(isignature);

			this->EvaluationStack->Push(new BoolStackItem(false));
			return;
		}

		// Read message

		// TODO: dangerous way to get the message

		byte* msg;
		int32 msgL = this->OnGetMessage(this->Iteration, msg);
		if (msgL <= 0)
		{
			IStackItem::Free(ipubKey);
			IStackItem::Free(isignature);

			this->EvaluationStack->Push(new BoolStackItem(false));
			return;
		}

		// Read public Key

		byte * pubKey = new byte[pubKeySize];
		pubKeySize = ipubKey->ReadByteArray(pubKey, 0, pubKeySize);

		// Read signature

		byte * signature = new byte[signatureSize];
		signatureSize = isignature->ReadByteArray(signature, 0, signatureSize);

		bool ret = Crypto::VerifySignature(msg, msgL, signature, signatureSize, pubKey, pubKeySize);

		delete[](pubKey);
		delete[](signature);

		IStackItem::Free(ipubKey);
		IStackItem::Free(isignature);

		this->EvaluationStack->Push(new BoolStackItem(ret));
		return;
	}
	case EVMOpCode::VERIFY:
	{
		if (this->EvaluationStack->Count() < 3)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* ipubKey = this->EvaluationStack->Pop();
		IStackItem* isignature = this->EvaluationStack->Pop();
		IStackItem* imsg = this->EvaluationStack->Pop();

		int pubKeySize = ipubKey->ReadByteArraySize();
		int signatureSize = isignature->ReadByteArraySize();
		int msgSize = imsg->ReadByteArraySize();

		if (pubKeySize < 33 || signatureSize < 32 || msgSize < 0)
		{
			IStackItem::Free(ipubKey);
			IStackItem::Free(isignature);
			IStackItem::Free(imsg);

			this->EvaluationStack->Push(new BoolStackItem(false));
			return;
		}

		// Read message

		byte * msg = new byte[msgSize];
		msgSize = imsg->ReadByteArray(msg, 0, msgSize);

		// Read public Key

		byte * pubKey = new byte[pubKeySize];
		pubKeySize = ipubKey->ReadByteArray(pubKey, 0, pubKeySize);

		// Read signature

		byte * signature = new byte[signatureSize];
		signatureSize = isignature->ReadByteArray(signature, 0, signatureSize);

		bool ret = Crypto::VerifySignature(msg, msgSize, signature, signatureSize, pubKey, pubKeySize);

		delete[](msg);
		delete[](pubKey);
		delete[](signature);

		IStackItem::Free(imsg);
		IStackItem::Free(ipubKey);
		IStackItem::Free(isignature);

		this->EvaluationStack->Push(new BoolStackItem(ret));
		return;
	}
	case EVMOpCode::CHECKMULTISIG:
	{
		int ic = this->EvaluationStack->Count();

		if (ic < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		byte ** pubKeys;
		byte ** signatures;
		int * pubKeysL;
		int * signaturesL;
		int pubKeysCount, signaturesCount;

		for (byte x = 0; x < 2; x++)
		{
			IStackItem* item = this->EvaluationStack->Pop();
			ic--;

			if (item->Type == EStackItemType::Array || item->Type == EStackItemType::Struct)
			{
				ArrayStackItem* arr = (ArrayStackItem*)item;
				int32 v = arr->Count();

				if (v <= 0)
				{
					this->State = EVMState::FAULT;
				}
				else
				{
					byte **data = new byte*[v];
					int32 *dataL = new int32[v];

					for (int32 i = 0; i < v; i++)
					{
						IStackItem* ret = arr->Get(i);

						int32 c = ret->ReadByteArraySize();
						if (c < 0)
						{
							data[i] = NULL;
							dataL[i] = c;
							continue;
						}

						data[i] = new byte[c];
						dataL[i] = ret->ReadByteArray(data[i], 0, c);
					}

					// Equal

					if (x == 0)
					{
						pubKeys = data;
						pubKeysL = dataL;
						pubKeysCount = v;
					}
					else
					{
						signatures = data;
						signaturesL = dataL;
						signaturesCount = v;
					}
				}
			}
			else
			{
				int32 v = 0;
				if (!item->GetInt32(v) || v < 1 || v > ic)
				{
					this->State = EVMState::FAULT;
				}
				else
				{
					byte **data = new byte*[v];
					int32 *dataL = new int32[v];

					for (int32 i = 0; i < v; i++)
					{
						IStackItem* ret = this->EvaluationStack->Pop();
						ic--;

						int32 c = ret->ReadByteArraySize();
						if (c < 0)
						{
							data[i] = NULL;
							dataL[i] = c;
							continue;
						}

						data[i] = new byte[c];
						dataL[i] = ret->ReadByteArray(data[i], 0, c);
						IStackItem::Free(ret);
					}

					// Equal

					if (x == 0)
					{
						pubKeys = data;
						pubKeysL = dataL;
						pubKeysCount = v;
					}
					else
					{
						signatures = data;
						signaturesL = dataL;
						signaturesCount = v;
					}
				}
			}

			IStackItem::Free(item);
		}

		if (this->State == EVMState::FAULT || this->OnGetMessage == NULL ||
			pubKeysCount <= 0 || signaturesCount <= 0 || signaturesCount > pubKeysCount)
		{
			if (pubKeys != NULL)	delete[](pubKeys);
			if (signatures != NULL) delete[](signatures);
			if (pubKeysL != NULL)	delete[](pubKeysL);
			if (signaturesL != NULL)delete[](signaturesL);

			this->EvaluationStack->Push(new BoolStackItem(false));
			return;
		}

		// Read message

		// TODO: dangerous way to get the message

		byte* msg;
		int32 msgL = this->OnGetMessage(this->Iteration, msg);
		if (msgL <= 0)
		{
			if (pubKeys != NULL)	delete[](pubKeys);
			if (signatures != NULL) delete[](signatures);
			if (pubKeysL != NULL)	delete[](pubKeysL);
			if (signaturesL != NULL)delete[](signaturesL);

			this->EvaluationStack->Push(new BoolStackItem(false));
			return;
		}

		bool fSuccess = true;
		for (int32 i = 0, j = 0; fSuccess && i < signaturesCount && j < pubKeysCount;)
		{
			if (Crypto::VerifySignature(msg, msgL, signatures[i], signaturesL[i], pubKeys[j], pubKeysL[j]))
				i++;

			j++;

			if (signaturesCount - i > pubKeysCount - j)
			{
				fSuccess = false;
				break;
			}
		}

		if (pubKeys != NULL)	delete[](pubKeys);
		if (signatures != NULL) delete[](signatures);
		if (pubKeysL != NULL)	delete[](pubKeysL);
		if (signaturesL != NULL)delete[](signaturesL);

		this->EvaluationStack->Push(new BoolStackItem(fSuccess));
		return;
	}

	// Array

	case EVMOpCode::ARRAYSIZE:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 size;
		IStackItem *item = this->EvaluationStack->Pop();

		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			ArrayStackItem *ar = (ArrayStackItem*)item;
			size = ar->Count();
			break;
		}
		case EStackItemType::Map:
		{
			MapStackItem *ar = (MapStackItem*)item;
			size = ar->Count();
			break;
		}
		default:
		{
			size = item->ReadByteArraySize();
			break;
		}
		}

		IStackItem::Free(item);

		if (size < 0)
		{
			this->State = EVMState::FAULT;
		}
		else
		{
			this->EvaluationStack->Push(new IntegerStackItem(size));
		}

		return;
	}
	case EVMOpCode::PACK:
	{
		int32 ec = this->EvaluationStack->Count();
		if (ec < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 size = 0;
		IStackItem *item = this->EvaluationStack->Pop();

		if (!item->GetInt32(size) || size < 0 || size >(ec - 1))
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(item);
		ArrayStackItem *items = new ArrayStackItem(false);

		for (int32 i = 0; i < size; i++)
		{
			items->Add(this->EvaluationStack->Pop());
		}

		this->EvaluationStack->Push(items);
		return;
	}
	case EVMOpCode::UNPACK:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();

		if (item->Type == EStackItemType::Array)
		{
			ArrayStackItem *array = (ArrayStackItem*)item;
			int32 count = array->Count();

			for (int32 i = count - 1; i >= 0; i--)
			{
				this->EvaluationStack->Push(array->Get(i));
			}

			IStackItem::Free(item);
			this->EvaluationStack->Push(new IntegerStackItem(count));
		}
		else
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}
		return;
	}
	case EVMOpCode::PICKITEM:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *key = this->EvaluationStack->Pop();

		if (key->Type == EStackItemType::Map ||
			key->Type == EStackItemType::Array ||
			key->Type == EStackItemType::Struct)
		{
			IStackItem::Free(key);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();
		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			ArrayStackItem *arr = (ArrayStackItem*)item;

			int32 index = 0;
			if (!key->GetInt32(index) || index < 0 || index >= arr->Count())
			{
				IStackItem::Free(key);
				IStackItem::Free(item);

				this->State = EVMState::FAULT;
				return;
			}

			IStackItem::Free(key);
			IStackItem *ret = arr->Get(index);
			this->EvaluationStack->Push(ret);
			IStackItem::Free(item);
			return;
		}
		case EStackItemType::Map:
		{
			MapStackItem* map = (MapStackItem*)item;

			IStackItem* val = map->Get(key);

			IStackItem::Free(key);
			IStackItem::Free(item);

			if (val != NULL)
			{
				this->EvaluationStack->Push(val);
			}
			else
			{
				this->State = EVMState::FAULT;
			}
			return;
		}
		default:
		{
			IStackItem::Free(key);
			IStackItem::Free(item);

			this->State = EVMState::FAULT;
			return;
		}
		}
		return;
	}
	case EVMOpCode::SETITEM:
	{
		if (this->EvaluationStack->Count() < 3)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *value = this->EvaluationStack->Pop();
		if (value->Type == EStackItemType::Struct)
		{
			IStackItem * clone = ((ArrayStackItem*)value)->Clone();
			IStackItem::Free(value);
			value = clone;
		}

		IStackItem *key = this->EvaluationStack->Pop();
		if (key->Type == EStackItemType::Map ||
			key->Type == EStackItemType::Array ||
			key->Type == EStackItemType::Struct)
		{
			IStackItem::Free(key);
			IStackItem::Free(value);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();
		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			ArrayStackItem *arr = (ArrayStackItem*)item;

			int32 index = 0;
			if (!key->GetInt32(index) || index < 0 || index >= arr->Count())
			{
				IStackItem::Free(key);
				IStackItem::Free(item);
				IStackItem::Free(value);

				this->State = EVMState::FAULT;
				return;
			}

			arr->Set(index, value);

			IStackItem::Free(key);
			IStackItem::Free(item);
			return;
		}
		case EStackItemType::Map:
		{
			MapStackItem *arr = (MapStackItem*)item;

			arr->Set(key, value);

			IStackItem::Free(key);
			IStackItem::Free(value);
			IStackItem::Free(item);
			return;
		}
		default:
		{
			IStackItem::Free(key);
			IStackItem::Free(value);
			IStackItem::Free(item);

			this->State = EVMState::FAULT;
			return;
		}
		}
	}
	case EVMOpCode::NEWARRAY:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 count = 0;
		IStackItem *item = this->EvaluationStack->Pop();

		if (!item->GetInt32(count) || count < 0)
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(item);
		this->EvaluationStack->Push(new ArrayStackItem(false, count));
		return;
	}
	case EVMOpCode::NEWSTRUCT:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 count = 0;
		IStackItem *item = this->EvaluationStack->Pop();

		if (!item->GetInt32(count) || count < 0)
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(item);
		this->EvaluationStack->Push(new ArrayStackItem(true, count));
		return;
	}
	case EVMOpCode::NEWMAP:
	{
		this->EvaluationStack->Push(new MapStackItem());
		return;
	}
	case EVMOpCode::APPEND:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *newItem = this->EvaluationStack->Pop();
		if (newItem->Type == EStackItemType::Struct)
		{
			IStackItem * clone = ((ArrayStackItem*)newItem)->Clone();
			IStackItem::Free(newItem);
			newItem = clone;
		}

		IStackItem *item = this->EvaluationStack->Pop();
		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			ArrayStackItem *arr = (ArrayStackItem*)item;
			arr->Add(newItem);
			IStackItem::Free(item);
			return;
		}
		default:
		{
			IStackItem::Free(newItem);
			IStackItem::Free(item);

			this->State = EVMState::FAULT;
			return;
		}
		}
		return;
	}
	case EVMOpCode::REVERSE:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();
		if (item->Type != EStackItemType::Array && item->Type != EStackItemType::Struct)
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}

		ArrayStackItem *arr = (ArrayStackItem*)item;
		arr->Reverse();
		IStackItem::Free(item);
		return;
	}
	case EVMOpCode::REMOVE:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *key = this->EvaluationStack->Pop();
		if (key->Type == EStackItemType::Map ||
			key->Type == EStackItemType::Array ||
			key->Type == EStackItemType::Struct)
		{
			IStackItem::Free(key);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();
		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			ArrayStackItem *arr = (ArrayStackItem*)item;

			int32 index = 0;
			if (!key->GetInt32(index) || index < 0 || index >= arr->Count())
			{
				IStackItem::Free(key);
				IStackItem::Free(item);

				this->State = EVMState::FAULT;
				return;
			}

			arr->RemoveAt(index);

			IStackItem::Free(key);
			IStackItem::Free(item);
			return;
		}
		case EStackItemType::Map:
		{
			MapStackItem *arr = (MapStackItem*)item;

			arr->Remove(key, true);

			IStackItem::Free(key);
			IStackItem::Free(item);
			return;
		}
		default:
		{
			IStackItem::Free(key);
			IStackItem::Free(item);

			this->State = EVMState::FAULT;
			return;
		}
		}
		return;
	}
	case EVMOpCode::HASKEY:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *key = this->EvaluationStack->Pop();
		if (key->Type == EStackItemType::Map ||
			key->Type == EStackItemType::Array ||
			key->Type == EStackItemType::Struct)
		{
			IStackItem::Free(key);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();
		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			ArrayStackItem *arr = (ArrayStackItem*)item;

			int32 index = 0;
			if (!key->GetInt32(index) || index < 0)
			{
				IStackItem::Free(key);
				IStackItem::Free(item);

				this->State = EVMState::FAULT;
				return;
			}

			this->EvaluationStack->Push(new BoolStackItem(index < arr->Count()));

			IStackItem::Free(key);
			IStackItem::Free(item);
			return;
		}
		case EStackItemType::Map:
		{
			MapStackItem *arr = (MapStackItem*)item;

			this->EvaluationStack->Push(new BoolStackItem(arr->Get(key) != NULL));

			IStackItem::Free(key);
			IStackItem::Free(item);
			return;
		}
		default:
		{
			IStackItem::Free(key);
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}
		}
		return;
	}
	case EVMOpCode::KEYS:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();
		switch (item->Type)
		{
		case EStackItemType::Map:
		{
			MapStackItem *arr = (MapStackItem*)item;

			ArrayStackItem *ap = new ArrayStackItem(false);
			arr->FillKeys(ap);
			this->EvaluationStack->Push(ap);

			IStackItem::Free(item);
			return;
		}
		default:
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}
		}
		return;
	}
	case EVMOpCode::VALUES:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();
		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			ArrayStackItem *arr = (ArrayStackItem*)item;
			this->EvaluationStack->Push(arr->Clone());
			IStackItem::Free(item);
			return;
		}
		case EStackItemType::Map:
		{
			MapStackItem *arr = (MapStackItem*)item;

			ArrayStackItem *ap = new ArrayStackItem(false);
			arr->FillValues(ap);
			this->EvaluationStack->Push(ap);

			IStackItem::Free(item);
			return;
		}
		default:
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}
		}
		return;
	}

	// Exceptions

	default:
	case EVMOpCode::THROW:
	{
		this->State = EVMState::FAULT;
		return;
	}
	case EVMOpCode::THROWIFNOT:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = this->EvaluationStack->Pop();

		if (!item->GetBoolean())
		{
			this->State = EVMState::FAULT;
		}

		IStackItem::Free(item);
		return;
	}

	}
}

// Destructor

ExecutionEngine::~ExecutionEngine()
{
	// Clean callbacks

	this->OnLoadScript = NULL;
	this->OnInvokeInterop = NULL;
	this->OnGetMessage = NULL;
	this->Log = NULL;

	// Clean pointers

	delete(this->InvocationStack);
	delete(this->EvaluationStack);
	delete(this->AltStack);
}