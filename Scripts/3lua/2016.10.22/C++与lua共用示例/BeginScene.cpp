#include "BeginScene.h"
#include "Frame.h"
#include "Main.h"

void CBeginScene::Init()
{
	m_Lua = luaL_newstate();

	//ע��Luaʹ�õ�C����
	lua_pushcfunction(m_Lua, c_message);
	lua_setglobal(m_Lua, "c_message");
	lua_pushcfunction(m_Lua, c_input);
	lua_setglobal(m_Lua, "c_input");
	lua_pushcfunction(m_Lua, c_draw_line);
	lua_setglobal(m_Lua, "c_draw_line");
	lua_pushcfunction(m_Lua, c_draw_rectangle);
	lua_setglobal(m_Lua, "c_draw_rectangle");
	lua_pushcfunction(m_Lua, c_draw_ellipse);
	lua_setglobal(m_Lua, "c_draw_ellipse");

	//װ��Lua�ļ�
	luaL_loadfile(m_Lua, "script/begin.lua");

	//ִ��Lua�ļ�
	lua_pcall(m_Lua, 0, 0, 0);

	//Lua��ʼ��
	lua_getglobal(m_Lua, "init");
	lua_pcall(m_Lua, 0, 0, 0);
}

void CBeginScene::Run()
{
	//Lua����
	lua_getglobal(m_Lua, "run");
	lua_pcall(m_Lua, 0, 0, 0);
}

void CBeginScene::End()
{
	lua_close(m_Lua);
}

void CBeginScene::Message(const char* msg)
{
	MessageBoxA(
		g_hWnd,
		msg,
		"BeginScene",
		MB_OK);
}

int CBeginScene::c_message(lua_State* lua)
{
	const char* msg = lua_tostring(lua, -1);

	//�����Ҫ����C++�����г�Ա������������
	//������취��ȡ�������ܵ��ã�����Ĵ�
	//�����ͨ���������������õ�����ָ�룬Ȼ
	//����ͨ������ָ����ó����ķǾ�̬��Ա��
	//���������lua����C++���г�Ա�����ķ�ʽ
	CBeginScene* bs = (CBeginScene*)CSceneManager::GetSceneManager()->GetScene(0);
	bs->Message(msg);

	return 0;
}

int CBeginScene::c_input(lua_State* lua)
{
	if (_KS_UC == g_GameInput->GetKeyState(VK_LBUTTON))
		lua_pushnumber(lua, 1);
	else
		lua_pushnumber(lua, 0);

	int x, y;
	g_GameInput->GetCursorPosition(&x, &y);
	lua_pushnumber(lua, x);
	lua_pushnumber(lua, y);

	return 3;
}

int CBeginScene::c_draw_line(lua_State* lua)
{
	int b = (int)lua_tonumber(lua, -1);
	int g = (int)lua_tonumber(lua, -2);
	int r = (int)lua_tonumber(lua, -3);
	int y2 = (int)lua_tonumber(lua, -4);
	int x2 = (int)lua_tonumber(lua, -5);
	int y1 = (int)lua_tonumber(lua, -6);
	int x1 = (int)lua_tonumber(lua, -7);
	g_GameOutput->DrawLine(
		x1, y1, x2, y2, RGB(r, g, b));
	return 0;
}

int CBeginScene::c_draw_rectangle(lua_State* lua)
{
	int b = (int)lua_tonumber(lua, -1);
	int g = (int)lua_tonumber(lua, -2);
	int r = (int)lua_tonumber(lua, -3);
	int y2 = (int)lua_tonumber(lua, -4);
	int x2 = (int)lua_tonumber(lua, -5);
	int y1 = (int)lua_tonumber(lua, -6);
	int x1 = (int)lua_tonumber(lua, -7);
	g_GameOutput->DrawRectangle(
		x1, y1, x2, y2, RGB(0, 0, 0), RGB(r, g, b));
	return 0;
}

int CBeginScene::c_draw_ellipse(lua_State* lua)
{
	int b = (int)lua_tonumber(lua, -1);
	int g = (int)lua_tonumber(lua, -2);
	int r = (int)lua_tonumber(lua, -3);
	int y2 = (int)lua_tonumber(lua, -4);
	int x2 = (int)lua_tonumber(lua, -5);
	int y1 = (int)lua_tonumber(lua, -6);
	int x1 = (int)lua_tonumber(lua, -7);
	g_GameOutput->DrawEllipse(
		x1, y1, x2, y2, RGB(0, 0, 0), RGB(r, g, b));
	return 0;
}