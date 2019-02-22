#include "GameInput.h"
#include <windows.h>

CGameInput::CGameInput(HWND hWnd)
:
m_hWnd(hWnd)
{
	//初始化所有按键的状态都是持续放开
	for (unsigned int i = 0; i < 256; ++i)
		m_KeyState[i] = _KS_UH;
}

void CGameInput::Run()
{
	//循环通过GetAsyncKeyState函数得到当前
	//指定的键是否被按下，再通过当前是否按
	//下和刚才状态是否按下来确定每个按键的
	//那4种状态
	for (unsigned int i = 0; i < 256; ++i)
	{
		//本次按下
		if (GetAsyncKeyState(i) & 0x8000)
		{
			//上次放开
			if (m_KeyState[i] < _KS_DH)
				//当前按下
				m_KeyState[i] = _KS_DC;
			//上次按下
			else
				//持续按下
				m_KeyState[i] = _KS_DH;
		}
		//本次放开
		else
		{
			//上次按下
			if (m_KeyState[i] > _KS_UC)
				//当前放开
				m_KeyState[i] = _KS_UC;
			//上次放开
			else
				//持续放开
				m_KeyState[i] = _KS_UH;
		}
	}
}

int CGameInput::GetKeyState(unsigned char VirtualKeyCode)
{
	//将VirtualKeyCode作为下标得到该键的状态
	return m_KeyState[VirtualKeyCode];
}

bool CGameInput::GetCursorPosition(int* X, int* Y)
{
	//得到当前光标在客户区的坐标
	POINT p;
	GetCursorPos(&p);
	ScreenToClient(m_hWnd, &p);
	*X = p.x;
	*Y = p.y;

	//返回当前光标是否是在客户区中
	RECT r;
	GetClientRect(m_hWnd, &r);
	return TRUE == PtInRect(&r, p);
}