#ifndef _FRAME_H_
#define _FRAME_H_

#define _CLIENT_PW 640
#define _CLIENT_PH 640

#include "GameInput.h"
#include "GameOutput.h"
#include "SceneManager.h"

extern CGameInput* g_GameInput;
extern CGameOutput* g_GameOutput;

void Init();
void Run();
void End();

#endif