#pragma once

// ExecutionEngine

typedef unsigned char(__stdcall * InvokeInteropCallback)(const char* method);
typedef int(__stdcall * GetScriptCallback)(const unsigned char* scriptHash, unsigned char* &script);
typedef int(__stdcall * GetMessageCallback)(unsigned int iteration, unsigned char* &message);