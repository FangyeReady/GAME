#ifndef _GAME_OUTPUT_H_
#define _GAME_OUTPUT_H_

#include <windows.h>
#include <vector>

class CGameOutput
{
	HWND m_hWnd; //����
	HDC m_MainDC; //���豸
	HDC m_BackDC; //���豸
	int m_ClientPW; //�ͻ�����
	int m_ClientPH; //�ͻ�����
	std::vector<HFONT> m_Fonts; //����
	std::vector<HDC> m_Bmps; //ͼƬ

public:
	
	//���������
	CGameOutput(HWND hWnd);
	~CGameOutput();

	//��ʼ
	void Begin();

	//����
	void End();

	//ͼԪ����

	//�߶λ���
	void DrawLine(
		int x1, int y1, //��ʼ��
		int x2, int y2, //��ֹ��
		int c); //��ɫ

	//���λ���
	void DrawRectangle(
		int x1, int y1, //���Ͻǵ�
		int x2, int y2, //���½ǵ�
		int c1, //�߿���ɫ
		int c2); //�����ɫ

	//Բ�λ���
	void DrawEllipse(
		int x1, int y1, //���Ͻǵ�
		int x2, int y2, //���½ǵ�
		int c1, //�߿���ɫ
		int c2); //�����ɫ

	//���ֻ���

	//�������֣�����ֵΪ���ֵ��±�
	int LoadFont(
		int w, //���ֿ�
		int h, //���ָ�
		const char* fn); //�������

	//��������
	bool DrawText(
		int i, //�����ڱ��е��±�
		int x, int y, //�������Ͻ�����
		const char* str, //��������
		int c); //������ɫ

	//λͼ����

	//����λͼ������ֵΪλͼ���±�
	int LoadBmp(
		const char* fn); //�ļ�����

	//����λͼ1
	bool DrawBmp1(
		int i, //λͼ�ڱ��е��±�
		int dx, int dy, int dw, int dh, //Ŀ�����
		int sx, int sy); //ԭê��

	//����λͼ2
	bool DrawBmp2(
		int i, //λͼ�ڱ��е��±�
		int dx, int dy, int dw, int dh, //Ŀ�����
		int sx, int sy, int sw, int sh, //ԭ����
		int tc); //͸��ɫ
};

#endif