#include "GameOutput.h"
#include <iostream>

#pragma comment(lib, "Msimg32.lib")

CGameOutput::CGameOutput(HWND hWnd)
:
m_hWnd(hWnd)
{
	//得到主设备
	m_MainDC = GetDC(m_hWnd);

	//得到窗口的客户区矩形
	RECT r;
	GetClientRect(m_hWnd, &r);
	m_ClientPW = r.right - r.left;
	m_ClientPH = r.bottom - r.top;

	//创建后备设备
	m_BackDC = CreateCompatibleDC(m_MainDC);
	HBITMAP hbmp = CreateCompatibleBitmap(m_MainDC, m_ClientPW, m_ClientPH);
	DeleteObject(SelectObject(m_BackDC, hbmp));

	//设置后备设备中文字背景透明
	SetBkMode(m_BackDC, TRANSPARENT);
}

CGameOutput::~CGameOutput()
{
	//释放位图
	for (int i = 0; i < (int)m_Bmps.size(); ++i)
		DeleteDC(m_Bmps[i]);

	//释放字体
	for (int i = 0; i < (int)m_Fonts.size(); ++i)
		DeleteObject(m_Fonts[i]);

	//删除后备设备
	DeleteDC(m_BackDC);

	//释放主设备
	ReleaseDC(m_hWnd, m_MainDC);
}

void CGameOutput::Begin()
{
	//将后备设备全部清白
	BitBlt(m_BackDC, 0, 0, m_ClientPW, m_ClientPH, 0, 0, 0, WHITENESS);
}

void CGameOutput::End()
{
	//二次缓冲
	BitBlt(m_MainDC, 0, 0, m_ClientPW, m_ClientPH, m_BackDC, 0, 0, SRCCOPY);
}

void CGameOutput::DrawLine(int x1,
						   int y1,
						   int x2,
						   int y2,
						   int c)
{
	//创建画笔
	HPEN p1 = CreatePen(PS_SOLID, 1, c);

	//选入设备
	HPEN p0 = (HPEN)SelectObject(m_BackDC, p1);

	//线段绘制
	MoveToEx(m_BackDC, x1, y1, 0);
	LineTo(m_BackDC, x2, y2);

	//选回老画笔
	SelectObject(m_BackDC, p0);

	//删除新画笔
	DeleteObject(p1);
}

void CGameOutput::DrawRectangle(int x1,
								int y1,
								int x2,
								int y2,
								int c1,
								int c2)
{
	//创建画笔选入设备
	HPEN p1 = CreatePen(PS_SOLID, 1, c1);
	HPEN p0 = (HPEN)SelectObject(m_BackDC, p1);

	//创建画刷选入设备
	HBRUSH b1 = CreateSolidBrush(c2);
	HBRUSH b0 = (HBRUSH)SelectObject(m_BackDC, b1);

	//矩形绘制
	Rectangle(m_BackDC, x1, y1, x2, y2);

	//选回老画笔、老画刷
	SelectObject(m_BackDC, b0);
	SelectObject(m_BackDC, p0);

	//删除新画笔画刷
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
	//创建画笔选入设备
	HPEN p1 = CreatePen(PS_SOLID, 1, c1);
	HPEN p0 = (HPEN)SelectObject(m_BackDC, p1);

	//创建画刷选入设备
	HBRUSH b1 = CreateSolidBrush(c2);
	HBRUSH b0 = (HBRUSH)SelectObject(m_BackDC, b1);

	//圆形绘制
	Ellipse(m_BackDC, x1, y1, x2, y2);

	//选回老画笔、老画刷
	SelectObject(m_BackDC, b0);
	SelectObject(m_BackDC, p0);

	//删除新画笔画刷
	DeleteObject(b1);
	DeleteObject(p1);
}

int CGameOutput::LoadFont(int w,
						  int h,
						  const char* fn)
{
	//创建字体
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

	//字体入表
	m_Fonts.push_back(f);

	//返回字体下标
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

	//选入新字体得到老字体
	HFONT f0 = (HFONT)SelectObject(m_BackDC, m_Fonts[i]);
	
	//设置新字体颜色得到老字体颜色
	int c0 = SetTextColor(m_BackDC, c);

	//绘制
	TextOutA(m_BackDC, x, y, str, (int)strlen(str));

	//还原老字体颜色
	SetTextColor(m_BackDC, c0);
	
	//还原老字体
	SelectObject(m_BackDC, f0);

	return true;
}

int CGameOutput::LoadBmp(const char* fn)
{
	//加载位图
	HBITMAP hbmp = (HBITMAP)LoadImageA(
		0,
		fn,
		IMAGE_BITMAP,
		0,
		0,
		LR_LOADFROMFILE);
	if (NULL == hbmp)
		return -1;

	//创建兼容设备
	HDC hbmpdc = CreateCompatibleDC(m_MainDC);

	//将位图选入兼容设备
	DeleteObject(SelectObject(hbmpdc, hbmp));

	//入表
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