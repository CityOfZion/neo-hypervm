#include "ExecutionEngine.h"
#include "ExecutionScript.h"
#include "ByteArrayStackItem.h"
#include "IntegerStackItem.h"
#include "BoolStackItem.h"
#include "MapStackItem.h"
#include "ArrayStackItem.h"
#include "Crypto.h"
#include "StackItemHelper.h"

// Setters

void ExecutionEngine::Clean(uint32 iteration)
{
	this->_iteration = iteration;
	this->_state = EVMState::NONE;
	this->_consumedGas = 0;
	this->_maxGas = 0xFFFFFFFF;

	this->InvocationStack.Clear();
	this->ResultStack.Clear();

	this->_counter->ItemCounterClean();
}

// Constructor

ExecutionEngine::ExecutionEngine
(
	InvokeInteropCallback &invokeInterop, LoadScriptCallback &loadScript, GetMessageCallback &getMessage
) :
	_iteration(0),
	_consumedGas(0),
	_maxGas(0xFFFFFFFF),
	_counter(new IStackItemCounter(MAX_STACK_SIZE)),
	_state(EVMState::NONE),
	Log(nullptr),

	OnGetMessage(getMessage),
	OnLoadScript(loadScript),
	OnInvokeInterop(invokeInterop),
	ResultStack(),
	InvocationStack()
{
	_counter->Claim();
}

// Destructor

ExecutionEngine::~ExecutionEngine()
{
	// Clean callbacks

	if (this->_counter->UnClaim())
	{
		delete(this->_counter);
	}

	// Clean stacks

	this->InvocationStack.Clear();
	this->ResultStack.Clear();
	this->Scripts.clear();
}

ExecutionContext* ExecutionEngine::LoadScript(std::shared_ptr<ExecutionScript> script, int32 rvcount)
{
	auto context = new ExecutionContext(script, 0, rvcount);
	this->InvocationStack.Push(context);
	return context;
}

bool ExecutionEngine::LoadScript(byte scriptIndex, int32 rvcount)
{
	std::shared_ptr<ExecutionScript> sc = nullptr;

	if (this->Scripts.size() > scriptIndex)
	{
		auto it = this->Scripts.begin();
		std::advance(it, scriptIndex);

		sc = (std::shared_ptr<ExecutionScript>)*it;
	}

	if (sc == nullptr) return false;

	auto context = new ExecutionContext(sc, 0, rvcount);
	this->InvocationStack.Push(context);
	return true;
}

int32 ExecutionEngine::LoadScript(byte* script, int32 scriptLength, int32 rvcount)
{
	int32 index = Scripts.size();

	auto sc = std::shared_ptr<ExecutionScript>(new ExecutionScript(script, scriptLength));
	Scripts.push_back(sc);

	auto context = new ExecutionContext(sc, 0, rvcount);
	this->InvocationStack.Push(context);
	return index;
}

EVMState ExecutionEngine::Execute(uint32 gas)
{
	this->_maxGas = gas;

	do
	{
		this->InternalStepInto();
	}
	while (this->_state == EVMState::NONE);

	return this->_state;
}

void ExecutionEngine::StepOut()
{
	int32 c = this->InvocationStack.Count();

	while (this->_state == EVMState::NONE && this->InvocationStack.Count() >= c)
	{
		this->InternalStepInto();
	}
}

void ExecutionEngine::StepOver()
{
	if (this->_state != EVMState::NONE) return;

	int32 c = this->InvocationStack.Count();

	do
	{
		this->InternalStepInto();
	}
	while (this->_state == EVMState::NONE && this->InvocationStack.Count() > c);
}

void ExecutionEngine::StepInto()
{
	if (this->_state == EVMState::NONE)
	{
		this->InternalStepInto();
	}
}

void ExecutionEngine::InternalStepInto()
{
	auto context = this->InvocationStack.Top();

	if (context == nullptr)
	{
		this->SetHalt();
		return;
	}

	if (this->Log != nullptr)
	{
		this->Log(context);
	}

	EVMOpCode opcode = context->ReadNextInstruction();

	// Execute opcode

	switch (opcode)
	{
		// Push value

	case EVMOpCode::PUSH0:
	{
		auto ret = this->CreateByteArray(nullptr, 0, false);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::PUSH1:
	case EVMOpCode::PUSH2:
	case EVMOpCode::PUSH3:
	case EVMOpCode::PUSH4:
	case EVMOpCode::PUSH5:
	case EVMOpCode::PUSH6:
	case EVMOpCode::PUSH7:
	case EVMOpCode::PUSH8:
	case EVMOpCode::PUSH9:
	case EVMOpCode::PUSH10:
	case EVMOpCode::PUSH11:
	case EVMOpCode::PUSH12:
	case EVMOpCode::PUSH13:
	case EVMOpCode::PUSH14:
	case EVMOpCode::PUSH15:
	case EVMOpCode::PUSH16:
	{
		auto ret = this->CreateInteger((opcode - EVMOpCode::PUSH1) + 1);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::PUSHBYTES1:
	case EVMOpCode::PUSHBYTES2:
	case EVMOpCode::PUSHBYTES3:
	case EVMOpCode::PUSHBYTES4:
	case EVMOpCode::PUSHBYTES5:
	case EVMOpCode::PUSHBYTES6:
	case EVMOpCode::PUSHBYTES7:
	case EVMOpCode::PUSHBYTES8:
	case EVMOpCode::PUSHBYTES9:
	case EVMOpCode::PUSHBYTES10:
	case EVMOpCode::PUSHBYTES11:
	case EVMOpCode::PUSHBYTES12:
	case EVMOpCode::PUSHBYTES13:
	case EVMOpCode::PUSHBYTES14:
	case EVMOpCode::PUSHBYTES15:
	case EVMOpCode::PUSHBYTES16:
	case EVMOpCode::PUSHBYTES17:
	case EVMOpCode::PUSHBYTES18:
	case EVMOpCode::PUSHBYTES19:
	case EVMOpCode::PUSHBYTES20:
	case EVMOpCode::PUSHBYTES21:
	case EVMOpCode::PUSHBYTES22:
	case EVMOpCode::PUSHBYTES23:
	case EVMOpCode::PUSHBYTES24:
	case EVMOpCode::PUSHBYTES25:
	case EVMOpCode::PUSHBYTES26:
	case EVMOpCode::PUSHBYTES27:
	case EVMOpCode::PUSHBYTES28:
	case EVMOpCode::PUSHBYTES29:
	case EVMOpCode::PUSHBYTES30:
	case EVMOpCode::PUSHBYTES31:
	case EVMOpCode::PUSHBYTES32:
	case EVMOpCode::PUSHBYTES33:
	case EVMOpCode::PUSHBYTES34:
	case EVMOpCode::PUSHBYTES35:
	case EVMOpCode::PUSHBYTES36:
	case EVMOpCode::PUSHBYTES37:
	case EVMOpCode::PUSHBYTES38:
	case EVMOpCode::PUSHBYTES39:
	case EVMOpCode::PUSHBYTES40:
	case EVMOpCode::PUSHBYTES41:
	case EVMOpCode::PUSHBYTES42:
	case EVMOpCode::PUSHBYTES43:
	case EVMOpCode::PUSHBYTES44:
	case EVMOpCode::PUSHBYTES45:
	case EVMOpCode::PUSHBYTES46:
	case EVMOpCode::PUSHBYTES47:
	case EVMOpCode::PUSHBYTES48:
	case EVMOpCode::PUSHBYTES49:
	case EVMOpCode::PUSHBYTES50:
	case EVMOpCode::PUSHBYTES51:
	case EVMOpCode::PUSHBYTES52:
	case EVMOpCode::PUSHBYTES53:
	case EVMOpCode::PUSHBYTES54:
	case EVMOpCode::PUSHBYTES55:
	case EVMOpCode::PUSHBYTES56:
	case EVMOpCode::PUSHBYTES57:
	case EVMOpCode::PUSHBYTES58:
	case EVMOpCode::PUSHBYTES59:
	case EVMOpCode::PUSHBYTES60:
	case EVMOpCode::PUSHBYTES61:
	case EVMOpCode::PUSHBYTES62:
	case EVMOpCode::PUSHBYTES63:
	case EVMOpCode::PUSHBYTES64:
	case EVMOpCode::PUSHBYTES65:
	case EVMOpCode::PUSHBYTES66:
	case EVMOpCode::PUSHBYTES67:
	case EVMOpCode::PUSHBYTES68:
	case EVMOpCode::PUSHBYTES69:
	case EVMOpCode::PUSHBYTES70:
	case EVMOpCode::PUSHBYTES71:
	case EVMOpCode::PUSHBYTES72:
	case EVMOpCode::PUSHBYTES73:
	case EVMOpCode::PUSHBYTES74:
	case EVMOpCode::PUSHBYTES75:
	{
		byte length = (byte)opcode;

		if (!this->AddGasCost())
		{
			return;
		}

		auto data = new byte[length];

		if (context->Read(data, length) != length)
		{
			delete[](data);
			this->SetFault();
			return;
		}

		auto ret = this->CreateByteArray(data, length, true);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::PUSHDATA1:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		byte length = 0;
		if (!context->ReadUInt8(length))
		{
			this->SetFault();
			return;
		}

		byte* data = new byte[length];
		if (context->Read(data, length) != length)
		{
			delete[](data);
			this->SetFault();
			return;
		}

		auto ret = this->CreateByteArray(data, length, true);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::PUSHDATA2:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		uint16 length = 0;
		if (!context->ReadUInt16(length))
		{
			this->SetFault();
			return;
		}

		byte* data = new byte[length];
		if (context->Read(data, length) != length)
		{
			delete[](data);
			this->SetFault();
			return;
		}

		auto ret = this->CreateByteArray(data, length, true);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::PUSHDATA4:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		int32 length = 0;
		if (!context->ReadInt32(length) || length < 0 || length > MAX_ITEM_LENGTH)
		{
			this->SetFault();
			return;
		}

		byte* data = new byte[length];
		if (context->Read(data, length) != length)
		{
			delete[](data);
			this->SetFault();
			return;
		}

		auto ret = this->CreateByteArray(data, length, true);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::PUSHM1:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		auto bi = new BigInteger(BigInteger::MinusOne);
		auto ret = this->CreateInteger(bi);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}

	// Control

	case EVMOpCode::NOP: return;
	case EVMOpCode::JMP:
	JmpLabel:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		int16 offset = 0;

		if (!context->ReadInt16(offset))
		{
			this->SetFault();
			return;
		}

		if (!context->SeekFromHere(offset - 3))
		{
			this->SetFault();
		}
		return;
	}
	case EVMOpCode::JMPIF:
	case EVMOpCode::JMPIFNOT:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		int16 offset = 0;

		if (context->EvaluationStack.Count() < 1 || !context->ReadInt16(offset))
		{
			this->SetFault();
			return;
		}

		auto it = context->EvaluationStack.Pop();

		if ((opcode == EVMOpCode::JMPIF) == it->GetBoolean())
		{
			if (!context->SeekFromHere(offset - 3))
			{
				// Do the same logic as official release

				context->EvaluationStack.Push(it);
				this->SetFault();
				return;
			}
		}
		else
		{
			if (!context->CouldSeekFromHere(offset - 3))
			{
				// Do the same logic as official release

				context->EvaluationStack.Push(it);
				this->SetFault();
				return;
			}
		}

		StackItemHelper::Free(it);
		return;
	}
	case EVMOpCode::CALL:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context == nullptr || this->InvocationStack.Count() >= MAX_INVOCATION_STACK_SIZE)
		{
			this->SetFault();
			return;
		}

		auto clone = context->Clone(-1, -1);
		this->InvocationStack.Push(clone);
		context->SeekFromHere(2);  // Official release don't check if is valid like JMP does

		// Jump

		opcode = EVMOpCode::JMP;
		context = clone;
		goto JmpLabel;
	}
	// Stack isolation (NEP8)
	case EVMOpCode::CALL_I:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context == nullptr || this->InvocationStack.Count() >= MAX_INVOCATION_STACK_SIZE)
		{
			this->SetFault();
			return;
		}

		byte rvcount = 0, pcount = 0;
		if (!context->ReadUInt8(rvcount) || !context->ReadUInt8(pcount))
		{
			this->SetFault();
			return;
		}

		if (context->EvaluationStack.Count() < pcount)
		{
			this->SetFault();
			return;
		}

		auto clone = context->Clone(rvcount, pcount);
		this->InvocationStack.Push(clone);

		context->SeekFromHere(2); // Official release don't check if is valid like JMP does

		// Jump

		opcode = EVMOpCode::JMP;
		context = clone;
		goto JmpLabel;
	}
	case EVMOpCode::CALL_E:
	case EVMOpCode::CALL_ED:
	case EVMOpCode::CALL_ET:
	case EVMOpCode::CALL_EDT:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (this->OnLoadScript == nullptr)
		{
			this->SetFault();
			return;
		}

		byte rvcount = 0, pcount = 0;
		if (!context->ReadUInt8(rvcount) || !context->ReadUInt8(pcount))
		{
			this->SetFault();
			return;
		}

		int32 ec = context->EvaluationStack.Count();
		if (ec < pcount)
		{
			this->SetFault();
			return;
		}

		if (opcode == EVMOpCode::CALL_ET || opcode == EVMOpCode::CALL_EDT)
		{
			if (context->RVCount != rvcount)
			{
				this->SetFault();
				return;
			}
		}

		const int32 scriptLength = 20;
		byte script_hash[scriptLength];

		bool isDynamicInvoke = opcode == EVMOpCode::CALL_ED || opcode == EVMOpCode::CALL_EDT;
		bool search = true;

		if (isDynamicInvoke)
		{
			// Require one element more

			if (ec < pcount + 1)
			{
				this->SetFault();
				return;
			}

			// Get hash from the evaluation stack

			auto it = context->EvaluationStack.Pop();
			int32 size = it->ReadByteArraySize();

			if (size != scriptLength || it->ReadByteArray(script_hash, 0, scriptLength) != scriptLength)
			{
				this->SetFault();
				StackItemHelper::UnclaimAndFree(it);
				return;
			}

			StackItemHelper::UnclaimAndFree(it);
		}
		else
		{
			if (context->Read(script_hash, scriptLength) != scriptLength)
			{
				this->SetFault();
				return;
			}

			// try to find in cache when is not dynamic call

			for (auto it = this->Scripts.begin(); it != this->Scripts.end(); ++it)
			{
				auto ptr = (std::shared_ptr<ExecutionScript>)*it;
				if (ptr->IsTheSameHash(script_hash, scriptLength))
				{
					this->LoadScript(ptr, rvcount);
					search = false;
					break;
				}
			}
		}

		if (search && this->OnLoadScript(script_hash, isDynamicInvoke ? 0x01 : 0x00, rvcount) != 0x01)
		{
			this->SetFault();
			return;
		}

		auto contextnew = this->InvocationStack.Top();
		context->EvaluationStack.SendTo(&contextnew->EvaluationStack, pcount);

		if (opcode == EVMOpCode::CALL_ET || opcode == EVMOpCode::CALL_EDT)
		{
			this->InvocationStack.Remove(1);
		}

		return;
	}
	case EVMOpCode::RET:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context != nullptr)
		{
			this->InvocationStack.Drop();

			int32 rvcount = context->RVCount;

			if (rvcount == -1)
			{
				rvcount = context->EvaluationStack.Count();
			}
			else
			{
				if (rvcount > 0 && context->EvaluationStack.Count() < rvcount)
				{
					this->SetFault();
					return;
				}
			}

			if (rvcount > 0)
			{
				if (this->InvocationStack.Count() == 0)
					context->EvaluationStack.SendTo(&this->ResultStack, rvcount);
				else
					context->EvaluationStack.SendTo(&this->GetCurrentContext()->EvaluationStack, rvcount);
			}
		}

		if (this->InvocationStack.Count() == 0)
		{
			this->SetHalt();
		}

		return;
	}
	case EVMOpCode::APPCALL:
	case EVMOpCode::TAILCALL:
	{
		if (!this->AddGasCost(10))
		{
			return;
		}

		if (this->OnLoadScript == nullptr)
		{
			this->SetFault();
			return;
		}

		const int32 scriptLength = 20;
		byte script_hash[scriptLength];
		if (context->Read(script_hash, scriptLength) != scriptLength)
		{
			this->SetFault();
			return;
		}

		bool isDynamicInvoke = true;
		for (int32 x = 0; x < scriptLength; ++x)
			if (script_hash[x] != 0x00) { isDynamicInvoke = false; break; }

		bool search = true;
		if (isDynamicInvoke)
		{
			if (context->EvaluationStack.Count() < 1)
			{
				this->SetFault();
				return;
			}

			auto item = context->EvaluationStack.Pop();
			if (item->ReadByteArray(&script_hash[0], 0, scriptLength) != scriptLength)
			{
				StackItemHelper::Free(item);
				this->SetFault();
				return;
			}

			StackItemHelper::Free(item);
		}
		else
		{
			// try to find in cache when is not dynamic call

			for (auto it = this->Scripts.begin(); it != this->Scripts.end(); ++it)
			{
				auto ptr = (std::shared_ptr<ExecutionScript>)*it;
				if (ptr->IsTheSameHash(script_hash, scriptLength))
				{
					this->LoadScript(ptr, -1);
					search = false;
					break;
				}
			}
		}

		if (search && this->OnLoadScript(script_hash, isDynamicInvoke ? 0x01 : 0x00, -1) != 0x01)
		{
			this->SetFault();
			return;
		}

		if (this->InvocationStack.Count() >= MAX_INVOCATION_STACK_SIZE)
		{
			this->SetFault();
			return;
		}

		context->EvaluationStack.SendTo(&this->GetCurrentContext()->EvaluationStack, -1);

		if (opcode == EVMOpCode::TAILCALL)
		{
			this->InvocationStack.Remove(1);
		}

		return;
	}
	case EVMOpCode::SYSCALL:
	{
		uint32 length = 0;
		if (this->OnInvokeInterop == nullptr || !context->ReadVarBytes(length, 252) || length == 0)
		{
			this->SetFault();
			return;
		}

		byte* data = new byte[length + 1];
		if (context->Read(data, length) != (int32)length)
		{
			delete[](data);
			this->SetFault();
			return;
		}

		data[length] = 0x00;

		if (this->OnInvokeInterop(data, (byte)length) != 0x01)
		{
			// Gas is computed outside, for this reason here could be "out of gas"

			if (this->_state != EVMState::FAULT_BY_GAS)
			{
				this->SetFault();
			}
		}

		delete[](data);
		return;
	}

	// Stack ops

	case EVMOpCode::DUPFROMALTSTACK:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->AltStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		context->EvaluationStack.Push(context->AltStack.Top());
		return;
	}
	case EVMOpCode::TOALTSTACK:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		context->AltStack.Push(context->EvaluationStack.Pop());
		return;
	}
	case EVMOpCode::FROMALTSTACK:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->AltStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		context->EvaluationStack.Push(context->AltStack.Pop());
		return;
	}
	case EVMOpCode::XDROP:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		int32 ic = context->EvaluationStack.Count();
		if (ic < 1)
		{
			this->SetFault();
			return;
		}

		auto it = context->EvaluationStack.Pop();

		int32 n = 0;
		if (!it->GetInt32(n) || n < 0 || n >= ic - 1)
		{
			StackItemHelper::Free(it);
			this->SetFault();
			return;
		}

		StackItemHelper::Free(it);
		it = context->EvaluationStack.Remove(n);
		StackItemHelper::Free(it);
		return;
	}
	case EVMOpCode::XSWAP:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		int32 ic = context->EvaluationStack.Count();
		if (ic < 1)
		{
			this->SetFault();
			return;
		}

		int32 n = 0;
		auto it = context->EvaluationStack.Pop();

		if (!it->GetInt32(n) || n < 0 || n >= ic - 1)
		{
			StackItemHelper::Free(it);
			this->SetFault();
			return;
		}

		StackItemHelper::Free(it);
		if (n == 0) return;

		auto xn = context->EvaluationStack.Peek(n);

		it = context->EvaluationStack.Remove(0);
		context->EvaluationStack.Insert(0, xn);
		context->EvaluationStack.Remove(n);
		context->EvaluationStack.Insert(n, it);
		return;
	}
	case EVMOpCode::XTUCK:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		int32 ic = context->EvaluationStack.Count();
		if (ic < 1)
		{
			this->SetFault();
			return;
		}

		int32 n = 0;
		auto it = context->EvaluationStack.Pop();

		if (!it->GetInt32(n) || n <= 0 || n > ic - 1)
		{
			StackItemHelper::Free(it);
			this->SetFault();
			return;
		}

		StackItemHelper::Free(it);
		context->EvaluationStack.Insert(n, context->EvaluationStack.Top());
		return;
	}
	case EVMOpCode::DEPTH:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		auto ret = this->CreateInteger(context->EvaluationStack.Count());

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::DROP:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		context->EvaluationStack.Drop();
		return;
	}
	case EVMOpCode::DUP:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		context->EvaluationStack.Push(context->EvaluationStack.Top());
		return;
	}
	case EVMOpCode::NIP:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x1 = context->EvaluationStack.Remove(1);
		StackItemHelper::Free(x1);
		return;
	}
	case EVMOpCode::OVER:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x1 = context->EvaluationStack.Peek(1);
		context->EvaluationStack.Push(x1);
		return;
	}
	case EVMOpCode::PICK:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		int32 ic = context->EvaluationStack.Count();
		if (ic < 1)
		{
			this->SetFault();
			return;
		}

		int32 n = 0;
		auto it = context->EvaluationStack.Pop();

		if (!it->GetInt32(n) || n < 0)
		{
			StackItemHelper::Free(it);
			this->SetFault();
			return;
		}

		StackItemHelper::Free(it);

		if (n >= ic - 1)
		{
			this->SetFault();
			return;
		}

		context->EvaluationStack.Push(context->EvaluationStack.Peek(n));
		return;
	}
	case EVMOpCode::ROLL:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		int32 ic = context->EvaluationStack.Count();
		if (ic < 1)
		{
			this->SetFault();
			return;
		}

		int32 n = 0;
		auto it = context->EvaluationStack.Pop();

		if (!it->GetInt32(n) || n < 0)
		{
			StackItemHelper::Free(it);
			this->SetFault();
			return;
		}

		StackItemHelper::Free(it);

		if (n >= ic - 1)
		{
			this->SetFault();
			return;
		}

		if (n == 0) return;

		context->EvaluationStack.Push(context->EvaluationStack.Remove(n));
		return;
	}
	case EVMOpCode::ROT:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 3)
		{
			this->SetFault();
			return;
		}

		auto x1 = context->EvaluationStack.Remove(2);
		context->EvaluationStack.Push(x1);
		return;
	}
	case EVMOpCode::SWAP:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x1 = context->EvaluationStack.Remove(1);
		context->EvaluationStack.Push(x1);
		return;
	}
	case EVMOpCode::TUCK:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x1 = context->EvaluationStack.Top();
		context->EvaluationStack.Insert(2, x1);
		return;
	}
	case EVMOpCode::CAT:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x2 = context->EvaluationStack.Pop();
		auto x1 = context->EvaluationStack.Pop();
		int32 size2 = x2->ReadByteArraySize();
		int32 size1 = x1->ReadByteArraySize();

		if (size2 < 0 || size1 < 0 || size1 + size2 > MAX_ITEM_LENGTH)
		{
			StackItemHelper::Free(x2, x1);

			this->SetFault();
			return;
		}

		byte* data = new byte[size2 + size1];
		x1->ReadByteArray(&data[0], 0, size1);
		x2->ReadByteArray(&data[size1], 0, size2);

		StackItemHelper::Free(x2, x1);

		auto ret = this->CreateByteArray(data, size1 + size2, true);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::SUBSTR:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 3)
		{
			this->SetFault();
			return;
		}

		int32 count = 0;
		auto it = context->EvaluationStack.Pop();

		if (!it->GetInt32(count) || count < 0)
		{
			StackItemHelper::Free(it);
			this->SetFault();
			return;
		}

		StackItemHelper::Free(it);
		it = context->EvaluationStack.Pop();
		int32 index = 0;

		if (!it->GetInt32(index) || index < 0)
		{
			StackItemHelper::Free(it);
			this->SetFault();
			return;
		}

		StackItemHelper::Free(it);
		it = context->EvaluationStack.Pop();

		byte* data = new byte[count];
		if (it->ReadByteArray(&data[0], index, count) != count)
		{
			delete[]data;
			StackItemHelper::Free(it);
			this->SetFault();
			return;
		}

		StackItemHelper::Free(it);

		auto ret = this->CreateByteArray(data, count, true);
		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::LEFT:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		int32 count = 0;
		auto it = context->EvaluationStack.Pop();

		if (!it->GetInt32(count) || count < 0)
		{
			StackItemHelper::Free(it);
			this->SetFault();
			return;
		}

		StackItemHelper::Free(it);
		it = context->EvaluationStack.Pop();

		byte* data = new byte[count];
		if (it->ReadByteArray(&data[0], 0, count) != count)
		{
			delete[]data;
			StackItemHelper::Free(it);
			this->SetFault();
			return;
		}

		StackItemHelper::Free(it);

		auto ret = this->CreateByteArray(data, count, true);
		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::RIGHT:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		int32 count = 0;
		auto it = context->EvaluationStack.Pop();

		if (!it->GetInt32(count) || count < 0)
		{
			StackItemHelper::Free(it);
			this->SetFault();
			return;
		}

		StackItemHelper::Free(it);
		it = context->EvaluationStack.Pop();

		byte* data = new byte[count];
		if (it->ReadByteArray(&data[0], it->ReadByteArraySize() - count, count) != count)
		{
			delete[]data;
			StackItemHelper::Free(it);
			this->SetFault();
			return;
		}

		StackItemHelper::Free(it);

		auto ret = this->CreateByteArray(data, count, true);
		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::SIZE:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		auto it = context->EvaluationStack.Pop();
		int32 size = it->ReadByteArraySize();
		StackItemHelper::Free(it);

		if (size < 0)
		{
			this->SetFault();
			return;
		}

		auto ret = this->CreateInteger(size);
		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}

	// Bitwise logic

	case EVMOpCode::INVERT:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		auto it = context->EvaluationStack.Pop();
		auto bi = it->GetBigInteger();
		StackItemHelper::Free(it);

		if (bi == nullptr)
		{
			this->SetFault();
			return;
		}

		auto reti = bi->Invert();
		delete(bi);

		if (reti == nullptr)
		{
			this->SetFault();
			return;
		}

		auto ret = this->CreateInteger(reti);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::AND:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x2 = context->EvaluationStack.Pop();
		auto x1 = context->EvaluationStack.Pop();

		auto i2 = x2->GetBigInteger();
		auto i1 = x1->GetBigInteger();

		StackItemHelper::Free(x1, x2);

		if (i2 == nullptr || i1 == nullptr)
		{
			if (i2 != nullptr) delete(i2);
			if (i1 != nullptr) delete(i1);

			this->SetFault();
			return;
		}

		auto reti = i1->And(i2);

		delete(i2);
		delete(i1);

		if (reti == nullptr)
		{
			this->SetFault();
			return;
		}

		auto ret = this->CreateInteger(reti);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::OR:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x2 = context->EvaluationStack.Pop();
		auto x1 = context->EvaluationStack.Pop();

		auto i2 = x2->GetBigInteger();
		auto i1 = x1->GetBigInteger();

		StackItemHelper::Free(x1, x2);

		if (i2 == nullptr || i1 == nullptr)
		{
			if (i2 != nullptr) delete(i2);
			if (i1 != nullptr) delete(i1);

			this->SetFault();
			return;
		}

		auto reti = i1->Or(i2);

		delete(i2);
		delete(i1);

		if (reti == nullptr)
		{
			this->SetFault();
			return;
		}

		auto ret = this->CreateInteger(reti);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::XOR:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x2 = context->EvaluationStack.Pop();
		auto x1 = context->EvaluationStack.Pop();

		auto i2 = x2->GetBigInteger();
		auto i1 = x1->GetBigInteger();

		StackItemHelper::Free(x1, x2);

		if (i2 == nullptr || i1 == nullptr)
		{
			if (i2 != nullptr) delete(i2);
			if (i1 != nullptr) delete(i1);

			this->SetFault();
			return;
		}

		auto reti = i1->Xor(i2);

		delete(i2);
		delete(i1);

		if (reti == nullptr)
		{
			this->SetFault();
			return;
		}

		auto ret = this->CreateInteger(reti);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::EQUAL:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x2 = context->EvaluationStack.Pop();
		auto x1 = context->EvaluationStack.Pop();
		auto ret = this->CreateBool(x1->Equals(x2));
		StackItemHelper::Free(x2, x1);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}

	// Numeric

	case EVMOpCode::INC:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		auto it = context->EvaluationStack.Pop();
		auto bi = it->GetBigInteger();
		StackItemHelper::Free(it);

		if (bi == nullptr)
		{
			this->SetFault();
			return;
		}

		if (bi->SizeExceeded())
		{
			delete(bi);
			this->SetFault();
			return;
		}

		auto add = new BigInteger(BigInteger::One);
		auto reti = bi->Add(add);
		delete(add);
		delete(bi);

		if (reti == nullptr)
		{
			this->SetFault();
			return;
		}

		if (reti->SizeExceeded())
		{
			delete(reti);
			this->SetFault();
			return;
		}

		auto ret = this->CreateInteger(reti);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::DEC:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		auto it = context->EvaluationStack.Pop();
		auto bi = it->GetBigInteger();
		StackItemHelper::Free(it);

		if (bi == nullptr)
		{
			this->SetFault();
			return;
		}

		if (bi->SizeExceeded())
		{
			delete(bi);
			this->SetFault();
			return;
		}

		auto add = new BigInteger(BigInteger::One);
		auto reti = bi->Sub(add);
		delete(add);
		delete(bi);

		if (reti == nullptr)
		{
			this->SetFault();
			return;
		}

		if (reti->SizeExceeded())
		{
			delete(reti);
			this->SetFault();
			return;
		}

		auto ret = this->CreateInteger(reti);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::SIGN:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		auto it = context->EvaluationStack.Pop();
		auto bi = it->GetBigInteger();
		StackItemHelper::Free(it);

		if (bi == nullptr)
		{
			this->SetFault();
			return;
		}

		int32 reti = bi->GetSign();
		delete(bi);

		auto ret = this->CreateInteger(reti);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::NEGATE:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		auto it = context->EvaluationStack.Pop();
		auto bi = it->GetBigInteger();
		StackItemHelper::Free(it);

		if (bi == nullptr)
		{
			this->SetFault();
			return;
		}

		auto reti = bi->Negate();
		delete(bi);

		if (reti == nullptr)
		{
			this->SetFault();
			return;
		}

		auto ret = this->CreateInteger(reti);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::ABS:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		auto it = context->EvaluationStack.Pop();
		auto bi = it->GetBigInteger();
		StackItemHelper::Free(it);

		if (bi == nullptr)
		{
			this->SetFault();
			return;
		}

		auto reti = bi->Abs();
		delete(bi);

		if (reti == nullptr)
		{
			this->SetFault();
			return;
		}

		auto ret = this->CreateInteger(reti);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::NOT:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		auto it = context->EvaluationStack.Pop();
		auto ret = this->CreateBool(!it->GetBoolean());
		StackItemHelper::Free(it);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}

		return;
	}
	case EVMOpCode::NZ:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		auto x = context->EvaluationStack.Pop();
		auto i = x->GetBigInteger();
		StackItemHelper::Free(x);

		if (i == nullptr)
		{
			this->SetFault();
			return;
		}

		auto ret = this->CreateBool(i->CompareTo(BigInteger::Zero) != 0);
		delete(i);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}

		return;
	}
	case EVMOpCode::ADD:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto i2 = context->EvaluationStack.Pop();
		auto i1 = context->EvaluationStack.Pop();
		auto x2 = i2->GetBigInteger();
		auto x1 = i1->GetBigInteger();
		StackItemHelper::Free(i2, i1);

		if (x2 == nullptr || x1 == nullptr ||
			x2->SizeExceeded() ||
			x1->SizeExceeded())
		{
			if (x2 != nullptr) delete(x2);
			if (x1 != nullptr) delete(x1);

			this->SetFault();
			return;
		}

		auto reti = x1->Add(x2);
		delete(x2);
		delete(x1);

		if (reti == nullptr)
		{
			this->SetFault();
			return;
		}

		if (reti->SizeExceeded())
		{
			delete(reti);
			this->SetFault();
			return;
		}

		auto ret = this->CreateInteger(reti);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::SUB:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto i2 = context->EvaluationStack.Pop();
		auto i1 = context->EvaluationStack.Pop();
		auto x2 = i2->GetBigInteger();
		auto x1 = i1->GetBigInteger();
		StackItemHelper::Free(i2, i1);

		if (x2 == nullptr || x1 == nullptr ||
			x2->SizeExceeded() ||
			x1->SizeExceeded())
		{
			if (x2 != nullptr) delete(x2);
			if (x1 != nullptr) delete(x1);

			this->SetFault();
			return;
		}

		auto reti = x1->Sub(x2);
		delete(x2);
		delete(x1);

		if (reti == nullptr)
		{
			this->SetFault();
			return;
		}

		if (reti->SizeExceeded())
		{
			delete(reti);
			this->SetFault();
			return;
		}

		auto ret = this->CreateInteger(reti);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::MUL:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto i2 = context->EvaluationStack.Pop();
		auto i1 = context->EvaluationStack.Pop();
		auto x2 = i2->GetBigInteger();
		auto x1 = i1->GetBigInteger();
		StackItemHelper::Free(i2, i1);

		if (
			x2 == nullptr || x1 == nullptr ||
			x2->ToByteArraySize() + x1->ToByteArraySize() > MAX_BIGINTEGER_SIZE
			)
		{
			if (x2 != nullptr) delete(x2);
			if (x1 != nullptr) delete(x1);

			this->SetFault();
			return;
		}

		auto reti = x1->Mul(x2);
		delete(x2);
		delete(x1);

		if (reti == nullptr)
		{
			this->SetFault();
			return;
		}

		auto ret = this->CreateInteger(reti);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::DIV:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto i2 = context->EvaluationStack.Pop();
		auto i1 = context->EvaluationStack.Pop();
		auto x2 = i2->GetBigInteger();
		auto x1 = i1->GetBigInteger();
		StackItemHelper::Free(i2, i1);

		if (x2 == nullptr || x1 == nullptr ||
			x1->SizeExceeded() ||
			x2->SizeExceeded())
		{
			if (x2 != nullptr) delete(x2);
			if (x1 != nullptr) delete(x1);

			this->SetFault();
			return;
		}

		auto reti = x1->Div(x2);
		delete(x2);
		delete(x1);

		if (reti == nullptr)
		{
			this->SetFault();
			return;
		}

		auto ret = this->CreateInteger(reti);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::MOD:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto i2 = context->EvaluationStack.Pop();
		auto i1 = context->EvaluationStack.Pop();
		auto x2 = i2->GetBigInteger();
		auto x1 = i1->GetBigInteger();
		StackItemHelper::Free(i2, i1);

		if (x2 == nullptr || x1 == nullptr ||
			x1->SizeExceeded() ||
			x2->SizeExceeded())
		{
			if (x2 != nullptr) delete(x2);
			if (x1 != nullptr) delete(x1);

			this->SetFault();
			return;
		}

		auto reti = x1->Mod(x2);
		delete(x2);
		delete(x1);

		if (reti == nullptr)
		{
			this->SetFault();
			return;
		}

		auto ret = this->CreateInteger(reti);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::SHL:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto n = context->EvaluationStack.Pop();
		auto x = context->EvaluationStack.Pop();
		auto in = n->GetBigInteger();
		auto ix = x->GetBigInteger();

		StackItemHelper::Free(n, x);

		int32 ishift;
		if (in == nullptr || ix == nullptr || !in->ToInt32(ishift) || (ishift > MAX_SHL_SHR || ishift < MIN_SHL_SHR))
		{
			if (ix != nullptr) delete(ix);
			if (in != nullptr) delete(in);

			this->SetFault();
			return;
		}

		delete(in);
		auto reti = ix->Shl(ishift);
		delete(ix);

		if (reti == nullptr)
		{
			this->SetFault();
			return;
		}

		if (reti->SizeExceeded())
		{
			delete(reti);
			this->SetFault();
			return;
		}

		auto ret = this->CreateInteger(reti);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::SHR:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto n = context->EvaluationStack.Pop();
		auto x = context->EvaluationStack.Pop();
		auto in = n->GetBigInteger();
		auto ix = x->GetBigInteger();

		StackItemHelper::Free(n, x);

		int32 ishift;
		if (in == nullptr || ix == nullptr || !in->ToInt32(ishift) || (ishift > MAX_SHL_SHR || ishift < MIN_SHL_SHR))
		{
			if (ix != nullptr) delete(ix);
			if (in != nullptr) delete(in);

			this->SetFault();
			return;
		}

		delete(in);
		auto reti = ix->Shr(ishift);
		delete(ix);

		if (reti == nullptr)
		{
			this->SetFault();
			return;
		}

		if (reti->SizeExceeded())
		{
			delete(reti);
			this->SetFault();
			return;
		}

		auto ret = this->CreateInteger(reti);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::BOOLAND:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x2 = context->EvaluationStack.Pop();
		auto x1 = context->EvaluationStack.Pop();
		auto ret = this->CreateBool(x1->GetBoolean() && x2->GetBoolean());
		StackItemHelper::Free(x2, x1);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::BOOLOR:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x2 = context->EvaluationStack.Pop();
		auto x1 = context->EvaluationStack.Pop();
		auto ret = this->CreateBool(x1->GetBoolean() || x2->GetBoolean());
		StackItemHelper::Free(x2, x1);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::NUMEQUAL:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x2 = context->EvaluationStack.Pop();
		auto x1 = context->EvaluationStack.Pop();

		auto i2 = x2->GetBigInteger();
		auto i1 = x1->GetBigInteger();

		StackItemHelper::Free(x1, x2);

		if (i2 == nullptr || i1 == nullptr)
		{
			if (i2 != nullptr) delete(i2);
			if (i1 != nullptr) delete(i1);

			this->SetFault();
			return;
		}

		auto ret = this->CreateBool(i1->CompareTo(i2) == 0);

		delete(i2);
		delete(i1);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::NUMNOTEQUAL:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x2 = context->EvaluationStack.Pop();
		auto x1 = context->EvaluationStack.Pop();

		auto i2 = x2->GetBigInteger();
		auto i1 = x1->GetBigInteger();

		StackItemHelper::Free(x1, x2);

		if (i2 == nullptr || i1 == nullptr)
		{
			if (i2 != nullptr) delete(i2);
			if (i1 != nullptr) delete(i1);

			this->SetFault();
			return;
		}

		auto ret = this->CreateBool(i1->CompareTo(i2) != 0);

		delete(i2);
		delete(i1);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::LT:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x2 = context->EvaluationStack.Pop();
		auto x1 = context->EvaluationStack.Pop();

		auto i2 = x2->GetBigInteger();
		auto i1 = x1->GetBigInteger();

		StackItemHelper::Free(x1, x2);

		if (i2 == nullptr || i1 == nullptr)
		{
			if (i2 != nullptr) delete(i2);
			if (i1 != nullptr) delete(i1);

			this->SetFault();
			return;
		}

		auto ret = this->CreateBool(i1->CompareTo(i2) < 0);

		delete(i1);
		delete(i2);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::GT:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x2 = context->EvaluationStack.Pop();
		auto x1 = context->EvaluationStack.Pop();

		auto i2 = x2->GetBigInteger();
		auto i1 = x1->GetBigInteger();

		StackItemHelper::Free(x1, x2);

		if (i2 == nullptr || i1 == nullptr)
		{
			if (i2 != nullptr) delete(i2);
			if (i1 != nullptr) delete(i1);

			this->SetFault();
			return;
		}

		auto ret = this->CreateBool(i1->CompareTo(i2) > 0);

		delete(i2);
		delete(i1);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::LTE:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x2 = context->EvaluationStack.Pop();
		auto x1 = context->EvaluationStack.Pop();

		auto i2 = x2->GetBigInteger();
		auto i1 = x1->GetBigInteger();

		StackItemHelper::Free(x1, x2);

		if (i2 == nullptr || i1 == nullptr)
		{
			if (i2 != nullptr) delete(i2);
			if (i1 != nullptr) delete(i1);

			this->SetFault();
			return;
		}

		auto ret = this->CreateBool(i1->CompareTo(i2) <= 0);

		delete(i2);
		delete(i1);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::GTE:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x2 = context->EvaluationStack.Pop();
		auto x1 = context->EvaluationStack.Pop();

		auto i2 = x2->GetBigInteger();
		auto i1 = x1->GetBigInteger();

		StackItemHelper::Free(x1, x2);

		if (i2 == nullptr || i1 == nullptr)
		{
			if (i2 != nullptr) delete(i2);
			if (i1 != nullptr) delete(i1);

			this->SetFault();
			return;
		}

		auto ret = this->CreateBool(i1->CompareTo(i2) >= 0);

		delete(i2);
		delete(i1);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::MIN:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x2 = context->EvaluationStack.Pop();
		auto x1 = context->EvaluationStack.Pop();

		auto i2 = x2->GetBigInteger();
		auto i1 = x1->GetBigInteger();

		StackItemHelper::Free(x1, x2);

		if (i2 == nullptr || i1 == nullptr)
		{
			if (i2 != nullptr) delete(i2);
			if (i1 != nullptr) delete(i1);

			this->SetFault();
			return;
		}

		IStackItem* ret;
		if (i1->CompareTo(i2) >= 0)
		{
			delete(i1);
			ret = this->CreateInteger(i2);
		}
		else
		{
			delete(i2);
			ret = this->CreateInteger(i1);
		}

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::MAX:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto x2 = context->EvaluationStack.Pop();
		auto x1 = context->EvaluationStack.Pop();

		auto i2 = x2->GetBigInteger();
		auto i1 = x1->GetBigInteger();

		StackItemHelper::Free(x1, x2);

		if (i2 == nullptr || i1 == nullptr)
		{
			if (i2 != nullptr) delete(i2);
			if (i1 != nullptr) delete(i1);

			this->SetFault();
			return;
		}

		IStackItem* ret;
		if (i1->CompareTo(i2) >= 0)
		{
			delete(i2);
			ret = this->CreateInteger(i1);
		}
		else
		{
			delete(i1);
			ret = this->CreateInteger(i2);
		}

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::WITHIN:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 3)
		{
			this->SetFault();
			return;
		}

		auto b = context->EvaluationStack.Pop();
		auto a = context->EvaluationStack.Pop();
		auto x = context->EvaluationStack.Pop();

		auto ib = b->GetBigInteger();
		auto ia = a->GetBigInteger();
		auto ix = x->GetBigInteger();

		StackItemHelper::Free(b, a, x);

		if (ib == nullptr || ia == nullptr || ix == nullptr)
		{
			if (ib != nullptr) delete(ib);
			if (ia != nullptr) delete(ia);
			if (ix != nullptr) delete(ix);

			this->SetFault();
			return;
		}

		auto ret = this->CreateBool(ia->CompareTo(ix) <= 0 && ix->CompareTo(ib) < 0);

		delete(ib);
		delete(ia);
		delete(ix);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}

	// Crypto

	case EVMOpCode::SHA1:
	{
		if (!this->AddGasCost(10))
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		auto item = context->EvaluationStack.Pop();
		int32 size = item->ReadByteArraySize();

		if (size < 0)
		{
			StackItemHelper::Free(item);
			this->SetFault();
			return;
		}

		byte* data = new byte[size];
		size = item->ReadByteArray(data, 0, size);
		StackItemHelper::Free(item);

		if (size < 0)
		{
			delete[]data;
			this->SetFault();
			return;
		}

		byte* hash = new byte[Crypto::SHA1_LENGTH];
		Crypto::ComputeSHA1(data, size, hash);
		delete[]data;

		auto ret = this->CreateByteArray(hash, Crypto::SHA1_LENGTH, true);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::SHA256:
	{
		if (!this->AddGasCost(10))
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		auto it = context->EvaluationStack.Pop();
		int32 size = it->ReadByteArraySize();

		if (size < 0)
		{
			StackItemHelper::Free(it);
			this->SetFault();
			return;
		}

		byte* data = new byte[size];
		size = it->ReadByteArray(data, 0, size);
		StackItemHelper::Free(it);

		if (size < 0)
		{
			delete[]data;
			this->SetFault();
			return;
		}

		byte* hash = new byte[Crypto::SHA256_LENGTH];
		Crypto::ComputeSHA256(data, size, hash);
		delete[]data;

		auto ret = this->CreateByteArray(hash, Crypto::SHA256_LENGTH, true);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::HASH160:
	{
		if (!this->AddGasCost(20))
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		auto item = context->EvaluationStack.Pop();
		int32 size = item->ReadByteArraySize();

		if (size < 0)
		{
			StackItemHelper::Free(item);
			this->SetFault();
			return;
		}

		byte* data = new byte[size];
		size = item->ReadByteArray(data, 0, size);
		StackItemHelper::Free(item);

		if (size < 0)
		{
			delete[]data;
			this->SetFault();
			return;
		}

		byte* hash = new byte[Crypto::HASH160_LENGTH];
		Crypto::ComputeHash160(data, size, hash);
		delete[]data;

		auto ret = this->CreateByteArray(hash, Crypto::HASH160_LENGTH, true);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::HASH256:
	{
		if (!this->AddGasCost(20))
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		auto it = context->EvaluationStack.Pop();
		int32 size = it->ReadByteArraySize();

		if (size < 0)
		{
			StackItemHelper::Free(it);
			this->SetFault();
			return;
		}

		byte* data = new byte[size];
		size = it->ReadByteArray(data, 0, size);
		StackItemHelper::Free(it);

		if (size < 0)
		{
			delete[]data;
			this->SetFault();
			return;
		}

		byte* hash = new byte[Crypto::HASH256_LENGTH];
		Crypto::ComputeHash256(data, size, hash);
		delete[]data;

		auto ret = this->CreateByteArray(hash, Crypto::HASH256_LENGTH, true);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}
	case EVMOpCode::CHECKSIG:
	{
		if (!this->AddGasCost(100))
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto ipubKey = context->EvaluationStack.Pop();
		auto isignature = context->EvaluationStack.Pop();

		int32 pubKeySize = ipubKey->ReadByteArraySize();
		int32 signatureSize = isignature->ReadByteArraySize();

		if (this->OnGetMessage == nullptr || pubKeySize < 33 || signatureSize < 32)
		{
			StackItemHelper::Free(ipubKey, isignature);

			auto ret = this->CreateBool(false);
			if (ret != nullptr)
			{
				context->EvaluationStack.Push(ret);
			}
			return;
		}

		// Read message

		// TODO: dangerous way to get the message

		byte* msg;
		int32 msgL = this->OnGetMessage(this->_iteration, msg);
		if (msgL <= 0)
		{
			StackItemHelper::Free(ipubKey, isignature);
			auto ret = this->CreateBool(false);
			if (ret != nullptr)
			{
				context->EvaluationStack.Push(ret);
			}
			return;
		}

		// Read public Key

		byte* pubKey = new byte[pubKeySize];
		pubKeySize = ipubKey->ReadByteArray(pubKey, 0, pubKeySize);

		// Read signature

		byte* signature = new byte[signatureSize];
		signatureSize = isignature->ReadByteArray(signature, 0, signatureSize);

		int16 ret = Crypto::VerifySignature(msg, msgL, signature, signatureSize, pubKey, pubKeySize);

		delete[](pubKey);
		delete[](signature);

		StackItemHelper::Free(ipubKey, isignature);

		auto retres = this->CreateBool(ret == 0x01);
		if (retres != nullptr)
		{
			context->EvaluationStack.Push(retres);
		}
		return;
	}
	case EVMOpCode::VERIFY:
	{
		if (!this->AddGasCost(100))
		{
			return;
		}

		if (context->EvaluationStack.Count() < 3)
		{
			this->SetFault();
			return;
		}

		auto ipubKey = context->EvaluationStack.Pop();
		auto isignature = context->EvaluationStack.Pop();
		auto imsg = context->EvaluationStack.Pop();

		int32 pubKeySize = ipubKey->ReadByteArraySize();
		int32 signatureSize = isignature->ReadByteArraySize();
		int32 msgSize = imsg->ReadByteArraySize();

		if (pubKeySize < 33 || signatureSize < 32 || msgSize < 0)
		{
			StackItemHelper::Free(ipubKey, isignature, imsg);
			auto ret = this->CreateBool(false);
			if (ret != nullptr)
			{
				context->EvaluationStack.Push(ret);
			}
			return;
		}

		// Read message

		byte* msg = new byte[msgSize];
		msgSize = imsg->ReadByteArray(msg, 0, msgSize);

		// Read public Key

		byte* pubKey = new byte[pubKeySize];
		pubKeySize = ipubKey->ReadByteArray(pubKey, 0, pubKeySize);

		// Read signature

		byte* signature = new byte[signatureSize];
		signatureSize = isignature->ReadByteArray(signature, 0, signatureSize);

		int16 ret = Crypto::VerifySignature(msg, msgSize, signature, signatureSize, pubKey, pubKeySize);

		delete[](msg);
		delete[](pubKey);
		delete[](signature);

		StackItemHelper::Free(imsg, ipubKey, isignature);

		auto retres = this->CreateBool(ret == 0x01);
		if (retres != nullptr)
		{
			context->EvaluationStack.Push(retres);
		}
		return;
	}
	case EVMOpCode::CHECKMULTISIG:
	{
		int32 ic = context->EvaluationStack.Count();

		if (ic < 2)
		{
			this->SetFault();
			return;
		}

		byte** pubKeys = nullptr;
		byte** signatures = nullptr;
		int32* pubKeysL = nullptr;
		int32* signaturesL = nullptr;
		int32 pubKeysCount = 0, signaturesCount = 0;

		for (byte x = 0; x < 2; ++x)
		{
			auto item = context->EvaluationStack.Pop();
			ic--;

			if (item->Type == EStackItemType::Array || item->Type == EStackItemType::Struct)
			{
				auto arr = (ArrayStackItem*)item;
				int32 v = arr->Count();

				if (v <= 0)
				{
					this->SetFault();
				}
				else
				{
					byte** data = new byte*[v];
					int32* dataL = new int32[v];

					for (int32 i = 0; i < v; ++i)
					{
						auto ret = arr->Get(i);

						int32 c = ret->ReadByteArraySize();
						if (c < 0)
						{
							data[i] = nullptr;
							dataL[i] = c;
							this->SetFault();
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
					this->SetFault();
				}
				else
				{
					byte** data = new byte*[v];
					int32* dataL = new int32[v];

					for (int32 i = 0; i < v; ++i)
					{
						auto ret = context->EvaluationStack.Pop();
						ic--;

						int32 c = ret->ReadByteArraySize();
						if (c < 0)
						{
							data[i] = nullptr;
							dataL[i] = c;
							this->SetFault();
							continue;
						}

						data[i] = new byte[c];
						dataL[i] = ret->ReadByteArray(data[i], 0, c);

						StackItemHelper::Free(ret);
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

			StackItemHelper::Free(item);
		}

		// Check fault

		if (pubKeysCount <= 0 || signaturesCount <= 0 || signaturesCount > pubKeysCount)
		{
			this->SetFault();
		}

		this->AddGasCost(100 * signaturesCount);

		if (this->_state != EVMState::NONE || this->OnGetMessage == nullptr)
		{
			// Free

			if (pubKeys != nullptr)	delete[](pubKeys);
			if (signatures != nullptr) delete[](signatures);
			if (pubKeysL != nullptr)	delete[](pubKeysL);
			if (signaturesL != nullptr)delete[](signaturesL);

			// Return

			if (this->_state == EVMState::NONE)
			{
				auto ret = this->CreateBool(false);

				if (ret != nullptr)
				{
					context->EvaluationStack.Push(ret);
				}
			}

			return;
		}

		// Read message

		// TODO: dangerous way to get the message

		byte* msg;
		int32 msgL = this->OnGetMessage(this->_iteration, msg);

		if (msgL <= 0)
		{
			if (pubKeys != nullptr)	delete[](pubKeys);
			if (signatures != nullptr) delete[](signatures);
			if (pubKeysL != nullptr)	delete[](pubKeysL);
			if (signaturesL != nullptr)delete[](signaturesL);

			auto ret = this->CreateBool(false);

			if (ret != nullptr)
			{
				context->EvaluationStack.Push(ret);
			}
			return;
		}

		bool fSuccess = true;
		for (int32 i = 0, j = 0; fSuccess && i < signaturesCount && j < pubKeysCount;)
		{
			if (Crypto::VerifySignature(msg, msgL, signatures[i], signaturesL[i], pubKeys[j], pubKeysL[j]))
				++i;

			j++;

			if (signaturesCount - i > pubKeysCount - j)
			{
				fSuccess = false;
				break;
			}
		}

		if (pubKeys != nullptr)	delete[](pubKeys);
		if (signatures != nullptr) delete[](signatures);
		if (pubKeysL != nullptr)	delete[](pubKeysL);
		if (signaturesL != nullptr)delete[](signaturesL);

		auto ret = this->CreateBool(fSuccess);

		if (ret != nullptr)
		{
			context->EvaluationStack.Push(ret);
		}
		return;
	}

	// Array

	case EVMOpCode::ARRAYSIZE:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		int32 size;
		auto item = context->EvaluationStack.Pop();

		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			auto ar = (ArrayStackItem*)item;
			size = ar->Count();
			break;
		}
		case EStackItemType::Map:
		{
			auto ar = (MapStackItem*)item;
			size = ar->Count();
			break;
		}
		default:
		{
			size = item->ReadByteArraySize();
			break;
		}
		}

		StackItemHelper::Free(item);

		if (size < 0)
		{
			this->SetFault();
		}
		else
		{
			auto ret = this->CreateInteger(size);
			if (ret != nullptr)
			{
				context->EvaluationStack.Push(ret);
			}
		}

		return;
	}
	case EVMOpCode::PACK:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		int32 ec = context->EvaluationStack.Count();
		if (ec < 1)
		{
			this->SetFault();
			return;
		}

		int32 size = 0;
		auto item = context->EvaluationStack.Pop();

		if (!item->GetInt32(size) || size < 0 || size >(ec - 1) || size > MAX_ARRAY_SIZE)
		{
			StackItemHelper::Free(item);
			this->SetFault();
			return;
		}

		StackItemHelper::Free(item);
		auto items = this->CreateArray();

		if (items == nullptr) return;

		for (int32 i = 0; i < size; ++i)
		{
			items->Add(context->EvaluationStack.Pop());
		}

		context->EvaluationStack.Push(items);
		return;
	}
	case EVMOpCode::UNPACK:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		auto it = context->EvaluationStack.Pop();

		if (it->Type == EStackItemType::Array)
		{
			auto array = (ArrayStackItem*)it;
			int32 count = array->Count();

			for (int32 i = count - 1; i >= 0; i--)
			{
				context->EvaluationStack.Push(array->Get(i));
			}

			StackItemHelper::Free(it);

			auto ret = this->CreateInteger(count);
			if (ret != nullptr)
			{
				context->EvaluationStack.Push(ret);
			}
		}
		else
		{
			StackItemHelper::Free(it);
			this->SetFault();
			return;
		}
		return;
	}
	case EVMOpCode::PICKITEM:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto key = context->EvaluationStack.Pop();

		if (key->Type == EStackItemType::Map ||
			key->Type == EStackItemType::Array ||
			key->Type == EStackItemType::Struct)
		{
			StackItemHelper::Free(key);
			this->SetFault();
			return;
		}

		IStackItem* ret = nullptr;
		auto item = context->EvaluationStack.Pop();

		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			auto arr = (ArrayStackItem*)item;

			int32 index = 0;
			if (!key->GetInt32(index) || index < 0 || index >= arr->Count())
			{
				break;
			}

			ret = arr->Get(index);
			break;
		}
		case EStackItemType::Map:
		{
			auto map = (MapStackItem*)item;
			ret = map->Get(key);
			break;
		}
		default: break;
		}

		if (ret != nullptr)
		{
			if (ret->Type == EStackItemType::Struct)
			{
				auto clone = ((ArrayStackItem*)ret)->Clone();
				StackItemHelper::Free(ret);
				ret = clone;
			}

			context->EvaluationStack.Push(ret);
			StackItemHelper::Free(key, item);
		}
		else
		{
			StackItemHelper::Free(key, item);
			this->SetFault();
		}

		return;
	}
	case EVMOpCode::SETITEM:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 3)
		{
			this->SetFault();
			return;
		}

		auto value = context->EvaluationStack.Pop();
		if (value->Type == EStackItemType::Struct)
		{
			auto clone = ((ArrayStackItem*)value)->Clone();
			StackItemHelper::Free(value);
			value = clone;
		}

		auto key = context->EvaluationStack.Pop();
		if (key->Type == EStackItemType::Map ||
			key->Type == EStackItemType::Array ||
			key->Type == EStackItemType::Struct)
		{
			StackItemHelper::Free(key, value);

			this->SetFault();
			return;
		}

		auto item = context->EvaluationStack.Pop();
		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			auto arr = (ArrayStackItem*)item;

			int32 index = 0;
			if (!key->GetInt32(index) || index < 0 || index >= arr->Count())
			{
				StackItemHelper::Free(key, item, value);

				this->SetFault();
				return;
			}

			arr->Set(index, value);

			StackItemHelper::Free(key, item);
			return;
		}
		case EStackItemType::Map:
		{
			auto arr = (MapStackItem*)item;

			if (arr->Set(key, value) && arr->Count() > MAX_ARRAY_SIZE)
			{
				// Overflow in one the MAX_ARRAY_SIZE, but is more optimized than check if exists before

				this->SetFault();
			}

			StackItemHelper::Free(key, value, item);
			return;
		}
		default:
		{
			StackItemHelper::Free(key, value, item);

			this->SetFault();
			return;
		}
		}
	}
	case EVMOpCode::NEWARRAY:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		int32 count = 0;
		auto item = context->EvaluationStack.Pop();

		if (!item->GetInt32(count) || count < 0 || count > MAX_ARRAY_SIZE)
		{
			StackItemHelper::Free(item);
			this->SetFault();
			return;
		}

		StackItemHelper::Free(item);

		auto it = this->CreateArray(count);
		if (it != nullptr)
		{
			context->EvaluationStack.Push(it);
		}
		return;
	}
	case EVMOpCode::NEWSTRUCT:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		int32 count = 0;
		auto item = context->EvaluationStack.Pop();

		if (!item->GetInt32(count) || count < 0 || count > MAX_ARRAY_SIZE)
		{
			StackItemHelper::Free(item);
			this->SetFault();
			return;
		}

		StackItemHelper::Free(item);

		auto it = this->CreateStruct(count);
		if (it != nullptr)
		{
			context->EvaluationStack.Push(it);
		}
		return;
	}
	case EVMOpCode::NEWMAP:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		auto it = this->CreateMap();

		if (it != nullptr)
		{
			context->EvaluationStack.Push(it);
		}
		return;
	}
	case EVMOpCode::APPEND:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto newItem = context->EvaluationStack.Pop();
		if (newItem->Type == EStackItemType::Struct)
		{
			auto clone = ((ArrayStackItem*)newItem)->Clone();
			StackItemHelper::Free(newItem);
			newItem = clone;
		}

		auto item = context->EvaluationStack.Pop();
		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			auto arr = (ArrayStackItem*)item;

			if ((arr->Count() + 1) > MAX_ARRAY_SIZE)
			{
				this->SetFault();
			}
			else
			{
				arr->Add(newItem);
			}

			StackItemHelper::Free(item);
			return;
		}
		default:
		{
			StackItemHelper::Free(newItem, item);

			this->SetFault();
			return;
		}
		}
		return;
	}
	case EVMOpCode::REVERSE:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		auto it = context->EvaluationStack.Pop();
		if (it->Type != EStackItemType::Array && it->Type != EStackItemType::Struct)
		{
			StackItemHelper::Free(it);
			this->SetFault();
			return;
		}

		auto arr = (ArrayStackItem*)it;
		arr->Reverse();
		StackItemHelper::Free(it);
		return;
	}
	case EVMOpCode::REMOVE:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto key = context->EvaluationStack.Pop();
		if (key->Type == EStackItemType::Map ||
			key->Type == EStackItemType::Array ||
			key->Type == EStackItemType::Struct)
		{
			StackItemHelper::Free(key);
			this->SetFault();
			return;
		}

		auto item = context->EvaluationStack.Pop();
		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			auto arr = (ArrayStackItem*)item;

			int32 index = 0;
			if (!key->GetInt32(index) || index < 0 || index >= arr->Count())
			{
				StackItemHelper::Free(key, item);

				this->SetFault();
				return;
			}

			arr->RemoveAt(index);

			StackItemHelper::Free(key, item);
			return;
		}
		case EStackItemType::Map:
		{
			auto arr = (MapStackItem*)item;

			arr->Remove(key);

			StackItemHelper::Free(key, item);
			return;
		}
		default:
		{
			StackItemHelper::Free(key, item);

			this->SetFault();
			return;
		}
		}
		return;
	}
	case EVMOpCode::HASKEY:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 2)
		{
			this->SetFault();
			return;
		}

		auto key = context->EvaluationStack.Pop();
		if (key->Type == EStackItemType::Map ||
			key->Type == EStackItemType::Array ||
			key->Type == EStackItemType::Struct)
		{
			StackItemHelper::Free(key);
			this->SetFault();
			return;
		}

		auto item = context->EvaluationStack.Pop();
		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			auto arr = (ArrayStackItem*)item;

			int32 index = 0;
			if (!key->GetInt32(index) || index < 0)
			{
				StackItemHelper::Free(key, item);

				this->SetFault();
				return;
			}

			auto ret = this->CreateBool(index < arr->Count());
			StackItemHelper::Free(key, item);

			if (ret != nullptr)
			{
				context->EvaluationStack.Push(ret);
			}
			return;
		}
		case EStackItemType::Map:
		{
			auto arr = (MapStackItem*)item;
			auto ret = this->CreateBool(arr->Get(key) != nullptr);
			StackItemHelper::Free(key, item);

			if (ret != nullptr)
			{
				context->EvaluationStack.Push(ret);
			}
			return;
		}
		default:
		{
			StackItemHelper::Free(key, item);
			this->SetFault();
			return;
		}
		}
		return;
	}
	case EVMOpCode::KEYS:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		auto it = context->EvaluationStack.Pop();
		switch (it->Type)
		{
		case EStackItemType::Map:
		{
			auto arr = (MapStackItem*)it;
			auto ap = this->CreateArray();

			if (ap != nullptr)
			{
				arr->FillKeys(ap);
				context->EvaluationStack.Push(ap);
			}

			StackItemHelper::Free(it);
			return;
		}
		default:
		{
			StackItemHelper::Free(it);
			this->SetFault();
			return;
		}
		}
		return;
	}
	case EVMOpCode::VALUES:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		auto it = context->EvaluationStack.Pop();
		switch (it->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			auto arr = (ArrayStackItem*)it;
			context->EvaluationStack.Push(arr->Clone());
			StackItemHelper::Free(it);
			return;
		}
		case EStackItemType::Map:
		{
			auto arr = (MapStackItem*)it;
			auto ap = this->CreateArray();

			if (ap != nullptr)
			{
				arr->FillValues(ap);
				context->EvaluationStack.Push(ap);
			}

			StackItemHelper::Free(it);
			return;
		}
		default:
		{
			StackItemHelper::Free(it);
			this->SetFault();
			return;
		}
		}
		return;
	}

	// Exceptions

	default:
	case EVMOpCode::THROW:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		this->SetFault();
		return;
	}
	case EVMOpCode::THROWIFNOT:
	{
		if (!this->AddGasCost())
		{
			return;
		}

		if (context->EvaluationStack.Count() < 1)
		{
			this->SetFault();
			return;
		}

		auto it = context->EvaluationStack.Pop();

		if (!it->GetBoolean())
		{
			this->SetFault();
		}

		StackItemHelper::Free(it);
		return;
	}
	}
}