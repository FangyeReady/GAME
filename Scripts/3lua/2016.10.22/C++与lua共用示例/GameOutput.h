#ifndef _GAME_OUTPUT_H_
#define _GAME_OUTPUT_H_

#include <windows.h>
#include <vector>

class CGameOutput
{
	HWND m_hWnd; //窗口
	HDC m_MainDC; //主设备
	HDC m_BackDC; //后备设备
	int m_ClientPW; //客户区宽
	int m_ClientPH; //客户区高
	std::vector<HFONT> m_Fonts; //文字
	std::vector<HDC> m_Bmps; //图片

public:
	
	//构造和析构
	CGameOutput(HWND hWnd);
	~CGameOutput();

	//开始
	void Begin();

	//结束
	void End();

	//图元绘制

	//线段绘制
	void DrawLine(
		int x1, int y1, //起始点
		int x2, int y2, //终止点
		int c); //颜色

	//矩形绘制
	void DrawRectangle(
		int x1, int y1, //左上角点
		int x2, int y2, //右下角点
		int c1, //边框颜色
		int c2); //填充颜色

	//圆形绘制
	void DrawEllipse(
		int x1, int y1, //左上角点
		int x2, int y2, //右下角点
		int c1, //边框颜色
		int c2); //填充颜色

	//文字绘制

	//加载文字，返回值为文字的下标
	int LoadFont(
		int w, //文字宽
		int h, //文字高
		const char* fn); //外观名称

	//绘制文字
	bool DrawText(
		int i, //文字在表中的下标
		int x, int y, //文字左上角坐标
		const char* str, //文字内容
		int c); //文字颜色

	//位图绘制

	//加载位图，返回值为位图的下标
	int LoadBmp(
		const char* fn); //文件名称

	//绘制位图1
	bool DrawBmp1(
		int i, //位图在表中的下标
		int dx, int dy, int dw, int dh, //目标矩形
		int sx, int sy); //原锚点

	//绘制位图2
	bool DrawBmp2(
		int i, //位图在表中的下标
		int dx, int dy, int dw, int dh, //目标矩形
		int sx, int sy, int sw, int sh, //原矩形
		int tc); //透明色
};

#endif