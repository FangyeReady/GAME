#ifndef _BEGIN_SCENE_H_
#define _BEGIN_SCENE_H_

#include "Scene.h"
#include "src/lua.hpp"

class CBeginScene : public CScene
{
	lua_State* m_Lua;

public:

	void Init();
	void Run();
	void End();

	//����CBeginScene�ĳ�Ա����
	void Message(const char* msg);

	//��������Ա�������з�װ
	static int c_message(lua_State* lua);

	//������غ���
	static int c_input(lua_State* lua);

	//�����غ���
	static int c_draw_line(lua_State* lua);
	static int c_draw_rectangle(lua_State* lua);
	static int c_draw_ellipse(lua_State* lua);
};

#endif