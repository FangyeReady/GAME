#include "Main.h"
#include "Frame.h"

//����
HWND g_hWnd = NULL;

//���ڼ����־
BOOL g_Active = TRUE;

//������Ϣ����
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
	//��䴰�����ṹ��
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

	//ע�ᴰ�����ṹ��
	RegisterClass(&wc);

	//���ݿͻ������μ�������ھ���
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

	//��������
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

	//��Ϸ��ʼ��
	Init();

	//��ʾ����
	ShowWindow(g_hWnd, SW_NORMAL);

	//���´���
	UpdateWindow(g_hWnd);

	//��Ϣѭ��
	MSG msg = {};
	while (msg.message != WM_QUIT)
	{
		if (PeekMessage(&msg, 0, 0, 0, PM_REMOVE))
		{
			//����õ���Ϣ��PeekMessage�ͻ᷵���棬��
			//ô�͵���DispatchMessage��ʹ�ô�����Ϣ��
			//����������Ϣ
			DispatchMessage(&msg);
		}
		else if (g_Active == TRUE)
		{
			//���û�д�����Ϣ�����Ǵ����Ǽ���״̬��ô
			//��ִ����Ϸѭ��������Ĵ�����Ϸ����
			Run();

			//Ĭ����Ϣ33���룬���ʾÿ��ִ��1000/33�
			//��30����Ϸѭ����һ��Ķ�ά��Ϸ����ÿ��25
			//�ε�40����Ϸѭ��
			Sleep(25);
		}
		else
		{
			//���û�д�����Ϣ�Ҵ�����δ����״̬����ô
			//���ǵĳ����Ӧ�õȴ���Ϣ��WaitMessage�ȴ�
			//��ǰ���߳���Ϣ�������Ƿ��������Ϣ��һ��
			//����WaitMessage���̷��أ����û�г�����ô
			//WaitMessageһֱ�ȴ��������ǵĴ��ڴ���δ��
			//��״̬��ʱ�����ǻ��յ�����Ϣ����WM_ACTIVATEAPP
			WaitMessage();
		}
	}

	//��Ϸ����
	End();
	
	return 1;
}

//GetMessage�������Ǵӵ�ǰ��Ϣ�����еõ�
//��ͷ��Ϣ�����û�ж�ͷ��Ϣ���ͻ�ȴ���
//ֻҪ�ǵȴ��Ͳ��������ǽ�����Ϸѭ������
//�������޸���Ϸѭ������Ҫ����ΪPeekMessage
//���������Ǵ���Ϣ�����еõ���ͷ��Ϣ����
//����û����Ϣ�����̷��أ�����Ϣ�ͷ�����
//��û����Ϣ�ͷ��ؼ٣�PeekMessage��ǰ4��
//������GetMessage��ȫ��ͬ�����һ������
//����ȷ���Ƿ�Ҫ����Ϣ������ɾ����Ϣ��GetMessage
//�õ���Ϣ֮���ֱ�ӴӶ�����ɾ����Ϣ����
//������ʹ��PeekMessage�Ļ�ҲҪɾ����Ϣ
//BOOL PeekMessage(LPMSG lpMsg, //���ڵõ���Ϣֵ����Ϣ����
//				 HWND hWnd, //Ŀ�괰�ڣ������0����Եõ����̴߳��������д��ڵ���Ϣ
//				 UINT wMsgFilterMin, //��Ϣ��������
//				 UINT wMsgFilterMax, //��Ϣ��������
//				 UINT wRemoveMsg); //�Ƿ�Ҫ����Ϣ������ɾ����Ϣ