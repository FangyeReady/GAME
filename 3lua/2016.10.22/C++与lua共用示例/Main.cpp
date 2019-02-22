#include "Main.h"
#include "Frame.h"

//窗口
HWND g_hWnd = NULL;

//窗口激活标志
BOOL g_Active = TRUE;

//窗口消息函数
LRESULT CALLBACK WindowProc(HWND hwnd,
							UINT uMsg,
							WPARAM wParam,
							LPARAM lParam)
{
	switch (uMsg)
	{
	case WM_ACTIVATEAPP:
		{
			g_Active = (BOOL)wParam;
			return 0;
		}
	case WM_DESTROY:
		{
			PostQuitMessage(0);
			return 0;
		}
	}
	return DefWindowProc(hwnd, uMsg, wParam, lParam);
}

int __stdcall WinMain(HINSTANCE hInstance,
					  HINSTANCE hPrevInstance,
					  LPSTR lpCmdLine,
					  int nCmdShow)
{
	//填充窗口类别结构体
	WNDCLASS wc;
	wc.style = CS_HREDRAW | CS_VREDRAW | CS_DBLCLKS;
	wc.lpfnWndProc = WindowProc;
	wc.cbClsExtra = 0;
	wc.cbWndExtra = 0;
	wc.hInstance = hInstance;
	wc.hIcon = LoadIcon(0, IDI_APPLICATION);
	wc.hCursor = LoadCursor(0, IDC_ARROW);
	wc.hbrBackground = (HBRUSH)COLOR_WINDOW;
	wc.lpszMenuName = 0;
	wc.lpszClassName = __TEXT("class-17");

	//注册窗口类别结构体
	RegisterClass(&wc);

	//根据客户区矩形计算机窗口矩形
	int sw = GetSystemMetrics(SM_CXSCREEN);
	int sh = GetSystemMetrics(SM_CYSCREEN);
	RECT r;
	r.left = (sw - _CLIENT_PW) / 2;
	r.top = (sh - _CLIENT_PH) / 2;
	r.right = r.left + _CLIENT_PW;
	r.bottom = r.top + _CLIENT_PH;
	AdjustWindowRect(
		&r,
		WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX,
		FALSE);

	//创建窗口
	g_hWnd = CreateWindow(
		wc.lpszClassName,
		__TEXT("s"),
		WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX,
		r.left,
		r.top,
		r.right - r.left,
		r.bottom - r.top,
		HWND_DESKTOP,
		0,
		wc.hInstance,
		0);

	//游戏初始化
	Init();

	//显示窗口
	ShowWindow(g_hWnd, SW_NORMAL);

	//更新窗口
	UpdateWindow(g_hWnd);

	//消息循环
	MSG msg = {};
	while (msg.message != WM_QUIT)
	{
		if (PeekMessage(&msg, 0, 0, 0, PM_REMOVE))
		{
			//如果得到消息则PeekMessage就会返回真，那
			//么就调用DispatchMessage来使用窗口消息函
			//数来处理消息
			DispatchMessage(&msg);
		}
		else if (g_Active == TRUE)
		{
			//如果没有窗口消息，但是窗口是激活状态那么
			//就执行游戏循环，下面的代码游戏运行
			Run();

			//默认休息33毫秒，则表示每秒执行1000/33差不
			//多30次游戏循环，一般的二维游戏都是每秒25
			//次到40次游戏循环
			Sleep(25);
		}
		else
		{
			//如果没有窗口消息且窗口是未激活状态，那么
			//我们的程序就应该等待消息，WaitMessage等待
			//当前的线程消息队列中是否出现了消息，一旦
			//出现WaitMessage立刻返回，如果没有出现那么
			//WaitMessage一直等待，当我们的窗口处于未激
			//活状态的时候，我们会收到的消息就是WM_ACTIVATEAPP
			WaitMessage();
		}
	}

	//游戏结束
	End();
	
	return 1;
}

//GetMessage的作用是从当前消息队列中得到
//队头消息，如果没有队头消息它就会等待，
//只要是等待就不利于我们进行游戏循环，所
//以我们修改游戏循环的主要函数为PeekMessage
//，其作用是从消息队列中得到队头消息，无
//论有没有消息都立刻返回，有消息就返回真
//，没有消息就返回假，PeekMessage的前4个
//参数和GetMessage完全相同，最后一个参数
//用于确定是否要从消息队列中删除消息；GetMessage
//得到消息之后会直接从队列中删除消息，所
//以我们使用PeekMessage的话也要删除消息
//BOOL PeekMessage(LPMSG lpMsg, //用于得到消息值的消息对象
//				 HWND hWnd, //目标窗口，如果是0则可以得到本线程创建的所有窗口的消息
//				 UINT wMsgFilterMin, //消息过滤下限
//				 UINT wMsgFilterMax, //消息过滤上限
//				 UINT wRemoveMsg); //是否要从消息队列中删除消息