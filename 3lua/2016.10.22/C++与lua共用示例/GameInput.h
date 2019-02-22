#ifndef _GAME_INPUT_H_
#define _GAME_INPUT_H_

#include <windows.h>

#define _KS_UH 1 //键状态:持续放开
#define _KS_UC 2 //键状态:当前放开
#define _KS_DH 3 //键状态:持续按下
#define _KS_DC 4 //键状态:当前按下

class CGameInput
{
private:

	//窗口
	HWND m_hWnd;

	//键的状态，因为Windows支持的按键最多就0xfe，所以
	//还不到256个，下面定义的数组就用于装载每个按键的
	//状态了，且直接用每个按键的键值作为数组下标
	int m_KeyState[256];

public:

	//构造
	CGameInput(HWND hWnd);

	//运行
	void Run();

	//得到某个键的状态
	int GetKeyState(unsigned char VirtualKeyCode);

	//得到当前光标在客户区的坐标，如果在客
	//户区就返回真，不在客户区中就返回假
	bool GetCursorPosition(int* X, int* Y);
};

#endif