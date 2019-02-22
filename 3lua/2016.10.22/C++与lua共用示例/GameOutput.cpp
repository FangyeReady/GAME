#include "GameOutput.h"
#include <iostream>

#pragma comment(lib, "Msimg32.lib")

CGameOutput::CGameOutput(HWND hWnd)
:
m_hWnd(hWnd)
{
	//�õ����豸
	m_MainDC = GetDC(m_hWnd);

	//�õ����ڵĿͻ�������
	RECT r;
	GetClientRect(m_hWnd, &r);
	m_ClientPW = r.right - r.left;
	m_ClientPH = r.bottom - r.top;

	//�������豸
	m_BackDC = CreateCompatibleDC(m_MainDC);
	HBITMAP hbmp = CreateCompatibleBitmap(m_MainDC, m_ClientPW, m_ClientPH);
	DeleteObject(SelectObject(m_BackDC, hbmp));

	//���ú��豸�����ֱ���͸��
	SetBkMode(m_BackDC, TRANSPARENT);
}

CGameOutput::~CGameOutput()
{
	//�ͷ�λͼ
	for (int i = 0; i < (int)m_Bmps.size(); ++i)
		DeleteDC(m_Bmps[i]);

	//�ͷ�����
	for (int i = 0; i < (int)m_Fonts.size(); ++i)
		DeleteObject(m_Fonts[i]);

	//ɾ�����豸
	DeleteDC(m_BackDC);

	//�ͷ����豸
	ReleaseDC(m_hWnd, m_MainDC);
}

void CGameOutput::Begin()
{
	//�����豸ȫ�����
	BitBlt(m_BackDC, 0, 0, m_ClientPW, m_ClientPH, 0, 0, 0, WHITENESS);
}

void CGameOutput::End()
{
	//���λ���
	BitBlt(m_MainDC, 0, 0, m_ClientPW, m_ClientPH, m_BackDC, 0, 0, SRCCOPY);
}

void CGameOutput::DrawLine(int x1,
						   int y1,
						   int x2,
						   int y2,
						   int c)
{
	//��������
	HPEN p1 = CreatePen(PS_SOLID, 1, c);

	//ѡ���豸
	HPEN p0 = (HPEN)SelectObject(m_BackDC, p1);

	//�߶λ���
	MoveToEx(m_BackDC, x1, y1, 0);
	LineTo(m_BackDC, x2, y2);

	//ѡ���ϻ���
	SelectObject(m_BackDC, p0);

	//ɾ���»���
	DeleteObject(p1);
}

void CGameOutput::DrawRectangle(int x1,
								int y1,
								int x2,
								int y2,
								int c1,
								int c2)
{
	//��������ѡ���豸
	HPEN p1 = CreatePen(PS_SOLID, 1, c1);
	HPEN p0 = (HPEN)SelectObject(m_BackDC, p1);

	//������ˢѡ���豸
	HBRUSH b1 = CreateSolidBrush(c2);
	HBRUSH b0 = (HBRUSH)SelectObject(m_BackDC, b1);

	//���λ���
	Rectangle(m_BackDC, x1, y1, x2, y2);

	//ѡ���ϻ��ʡ��ϻ�ˢ
	SelectObject(m_BackDC, b0);
	SelectObject(m_BackDC, p0);

	//ɾ���»��ʻ�ˢ
	DeleteObject(b1);
	DeleteObject(p1);
}

void CGameOutput::DrawEllipse(int x1,
							  int y1,
							  int x2,
							  int y2,
							  int c1,
							  int c2)
{
	//��������ѡ���豸
	HPEN p1 = CreatePen(PS_SOLID, 1, c1);
	HPEN p0 = (HPEN)SelectObject(m_BackDC, p1);

	//������ˢѡ���豸
	HBRUSH b1 = CreateSolidBrush(c2);
	HBRUSH b0 = (HBRUSH)SelectObject(m_BackDC, b1);

	//Բ�λ���
	Ellipse(m_BackDC, x1, y1, x2, y2);

	//ѡ���ϻ��ʡ��ϻ�ˢ
	SelectObject(m_BackDC, b0);
	SelectObject(m_BackDC, p0);

	//ɾ���»��ʻ�ˢ
	DeleteObject(b1);
	DeleteObject(p1);
}

int CGameOutput::LoadFont(int w,
						  int h,
						  const char* fn)
{
	//��������
	HFONT f = CreateFont(
		h,
		w,
		0,
		0,
		FW_NORMAL,
		false,
		false,
		false,
		DEFAULT_CHARSET,
		OUT_DEFAULT_PRECIS,
		CLIP_DEFAULT_PRECIS,
		DEFAULT_QUALITY,
		DEFAULT_PITCH | FF_DONTCARE,
		fn);
	if (NULL == f)
		return -1;

	//�������
	m_Fonts.push_back(f);

	//���������±�
	return (int)m_Fonts.size() - 1;
}

bool CGameOutput::DrawText(int i,
						   int x,
						   int y,
						   const char* str,
						   int c)
{
	if (i < 0 || i >= (int)m_Fonts.size())
		return false;

	//ѡ��������õ�������
	HFONT f0 = (HFONT)SelectObject(m_BackDC, m_Fonts[i]);
	
	//������������ɫ�õ���������ɫ
	int c0 = SetTextColor(m_BackDC, c);

	//����
	TextOutA(m_BackDC, x, y, str, (int)strlen(str));

	//��ԭ��������ɫ
	SetTextColor(m_BackDC, c0);
	
	//��ԭ������
	SelectObject(m_BackDC, f0);

	return true;
}

int CGameOutput::LoadBmp(const char* fn)
{
	//����λͼ
	HBITMAP hbmp = (HBITMAP)LoadImageA(
		0,
		fn,
		IMAGE_BITMAP,
		0,
		0,
		LR_LOADFROMFILE);
	if (NULL == hbmp)
		return -1;

	//���������豸
	HDC hbmpdc = CreateCompatibleDC(m_MainDC);

	//��λͼѡ������豸
	DeleteObject(SelectObject(hbmpdc, hbmp));

	//���
	m_Bmps.push_back(hbmpdc);

	return (int)m_Bmps.size() - 1;
}

bool CGameOutput::DrawBmp1(int i,
						   int dx,
						   int dy,
						   int dw,
						   int dh,
						   int sx,
						   int sy)
{
	if (i < 0 || i >= (int)m_Bmps.size())
		return false;

	BitBlt(m_BackDC, dx, dy, dw, dh, m_Bmps[i], sx, sy, SRCCOPY);

	return true;
}

bool CGameOutput::DrawBmp2(int i,
						   int dx,
						   int dy,
						   int dw,
						   int dh,
						   int sx,
						   int sy,
						   int sw,
						   int sh,
						   int tc)
{
	if (i < 0 || i >= (int)m_Bmps.size())
		return false;

	TransparentBlt(
		m_BackDC,
		dx, dy, dw, dh,
		m_Bmps[i],
		sx, sy, sw, sh,
		tc);

	return true;
}