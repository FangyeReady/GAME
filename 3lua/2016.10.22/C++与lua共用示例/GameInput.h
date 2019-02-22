#ifndef _GAME_INPUT_H_
#define _GAME_INPUT_H_

#include <windows.h>

#define _KS_UH 1 //��״̬:�����ſ�
#define _KS_UC 2 //��״̬:��ǰ�ſ�
#define _KS_DH 3 //��״̬:��������
#define _KS_DC 4 //��״̬:��ǰ����

class CGameInput
{
private:

	//����
	HWND m_hWnd;

	//����״̬����ΪWindows֧�ֵİ�������0xfe������
	//������256�������涨������������װ��ÿ��������
	//״̬�ˣ���ֱ����ÿ�������ļ�ֵ��Ϊ�����±�
	int m_KeyState[256];

public:

	//����
	CGameInput(HWND hWnd);

	//����
	void Run();

	//�õ�ĳ������״̬
	int GetKeyState(unsigned char VirtualKeyCode);

	//�õ���ǰ����ڿͻ��������꣬����ڿ�
	//�����ͷ����棬���ڿͻ����оͷ��ؼ�
	bool GetCursorPosition(int* X, int* Y);
};

#endif