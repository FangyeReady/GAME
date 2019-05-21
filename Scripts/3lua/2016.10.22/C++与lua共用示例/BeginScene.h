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

	//属于CBeginScene的成员函数
	void Message(const char* msg);

	//对上述成员函数进行封装
	static int c_message(lua_State* lua);

	//输入相关函数
	static int c_input(lua_State* lua);

	//输出相关函数
	static int c_draw_line(lua_State* lua);
	static int c_draw_rectangle(lua_State* lua);
	static int c_draw_ellipse(lua_State* lua);
};

#endif