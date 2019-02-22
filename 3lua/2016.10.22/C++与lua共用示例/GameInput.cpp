#include "GameInput.h"
#include <windows.h>

CGameInput::CGameInput(HWND hWnd)
:
m_hWnd(hWnd)
{
	//��ʼ�����а�����״̬���ǳ����ſ�
	for (unsigned int i = 0; i < 256; ++i)
		m_KeyState[i] = _KS_UH;
}

void CGameInput::Run()
{
	//ѭ��ͨ��GetAsyncKeyState�����õ���ǰ
	//ָ���ļ��Ƿ񱻰��£���ͨ����ǰ�Ƿ�
	//�º͸ղ�״̬�Ƿ�����ȷ��ÿ��������
	//��4��״̬
	for (unsigned int i = 0; i < 256; ++i)
	{
		//���ΰ���
		if (GetAsyncKeyState(i) & 0x8000)
		{
			//�ϴηſ�
			if (m_KeyState[i] < _KS_DH)
				//��ǰ����
				m_KeyState[i] = _KS_DC;
			//�ϴΰ���
			else
				//��������
				m_KeyState[i] = _KS_DH;
		}
		//���ηſ�
		else
		{
			//�ϴΰ���
			if (m_KeyState[i] > _KS_UC)
				//��ǰ�ſ�
				m_KeyState[i] = _KS_UC;
			//�ϴηſ�
			else
				//�����ſ�
				m_KeyState[i] = _KS_UH;
		}
	}
}

int CGameInput::GetKeyState(unsigned char VirtualKeyCode)
{
	//��VirtualKeyCode��Ϊ�±�õ��ü���״̬
	return m_KeyState[VirtualKeyCode];
}

bool CGameInput::GetCursorPosition(int* X, int* Y)
{
	//�õ���ǰ����ڿͻ���������
	POINT p;
	GetCursorPos(&p);
	ScreenToClient(m_hWnd, &p);
	*X = p.x;
	*Y = p.y;

	//���ص�ǰ����Ƿ����ڿͻ�����
	RECT r;
	GetClientRect(m_hWnd, &r);
	return TRUE == PtInRect(&r, p);
}