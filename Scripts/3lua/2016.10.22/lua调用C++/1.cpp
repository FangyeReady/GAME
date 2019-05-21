#include <iostream>
#include "src/lua.hpp"

//luaҪ����õ�C���Ժ��������ӱ����Ƿ���ֵΪint����ʽ
//������Ϊһ��lua_Stateָ�룬lua�ڵ���ע���C���Ժ���
//��ʱ�򣬻��ջ��գ�Ȼ��ѵ��õĲ������η��뵽ջ�У�
//������C���Ժ����оͿ��Եõ�ջ�еĶ���������ݣ����
//����C���Դ������ˣ�����д�����Ҫ���ݸ�lua�Ļ�����
//��Ҫͨ������ֵ��֪lua���Ƿ��˶��ٸ�������ջ�У�lua��
//�õ�ʱ��Ϳ���ͨ�����ط���ֵ�õ���Щ���������

int dy(lua_State* ls)
{
	std::cout<<lua_gettop(ls)<<std::endl;
	std::cout<<lua_tostring(ls, -1)<<std::endl;
	std::cout<<lua_tostring(ls, -2)<<std::endl;
	std::cout<<lua_tostring(ls, -3)<<std::endl;
	return 0;
}

int dy_i(lua_State* ls)
{
	std::cout<<lua_tonumber(ls, -1)<<std::endl;
	return 0;
}

int cl(lua_State* ls)
{
	double i2 = lua_tonumber(ls, -1);
	double i1 = lua_tonumber(ls, -2);
	lua_settop(ls, 0);
	lua_pushnumber(ls, i1 + i2);
	lua_pushnumber(ls, i1 - i2);
	lua_pushnumber(ls, i1 * i2);
	lua_pushnumber(ls, i1 / i2);
	
	return 3;
}

void main()
{
	lua_State* ls = luaL_newstate();

	lua_pushcfunction(ls, dy);
	//[dy����]�ס���

	lua_setglobal(ls, "c_dy");
	//[]�ס���
	//��lua��ȫ�����о�����һ������c_dy�ĺ���

	lua_pushcfunction(ls, dy_i);
	lua_setglobal(ls, "c_dy_i");
	lua_pushcfunction(ls, cl);
	lua_setglobal(ls, "c_cl");

	luaL_loadfile(ls, "test2.lua");
	lua_pcall(ls, 0, 0, 0);

	lua_close(ls);

	system("pause");
}

//����lua���л���
//luaL_newstate()

//װ��lua�ļ�������﷨����װ�����֮������lua�ļ�
//���൱��һ����ġ�������lua���������ڵ�ǰ��lua���л�
//����ջ��
//luaL_loadfile(lua_State* lua���л���, char* lua�ļ�)

//װ�ذ���luaԴ������ַ���������﷨����
//luaL_loadstring(lua_State* lua���л���, char* ����luaԴ������ַ���)

//�ر�lua���л���
//lua_close(lua_State* lua���л���)

//lua��c++�Ľ�������Ҫ����һ��ջ����ɣ����
//ջ���һ�������ϵ�ջ���Ը�����һ��˳���
//�Ӵ���lua���л���֮�����ջҲ�ͱ�����������
//����Ȼ�Ƿ���lua���л����еģ��������ջ����
//����ʹ������������������ջ��Ԫ�ؽ��з��ʣ�
//���ʵĹ���������ʾ��
//+4[��]-1
//+3[..]-2
//+2[..]-3
//+1[��]-4
//���������ջ��Ϊ��׼��ʼ����ջ��Ԫ�أ�����
//������Ϊ�±꣬-1����ջ����-2����ջ�������
//һ��Ԫ�أ��Դ����ƣ����������ջ��Ϊ��׼��
//ʼ����ջ��Ԫ�أ�����������Ϊ�±꣬1����ջ��
//��2����ջ������һ��Ԫ�أ��Դ����ơ�

//�õ���ǰlua���л�����ջ������ջ�׵ĳ���
//lua_gettop(lua_State* lua���л���)

//���õ�ǰջ����λ��Ϊnum�����統ǰջ����
//�����ջ����5��Ԫ�أ������������Ĵ���
//Ϊlua_settop(ls, 3)����5��Ԫ���е�����
//������Ԫ�ص�������ջ�о�ֻʣ��3��Ԫ��
//lua_settop(lua_State* lua���л���, int num)

//����һ��������lua���л�����ջ��
//lua_pushinteger(lua_State* lua���л���, int i)

//����һ����������lua���л�����ջ��
//lua_pushnumber(lua_State* lua���л���, double d)

//����һ���ַ�����lua���л�����ջ��
//lua_pushstring(lua_State* lua���л���, const char* str)

//����һ��C���Ժ�����lua���л�����ջ����ע���C���Ժ�������������
//int ������(lua_State* lua���л���)
//lua_pushcfunction(lua_State* lua���л���, int (*f)(lua_State*))

//��ָ��ջ������ջ��Ԫ��ת��Ϊ������
//lua_tonumber(lua_State* lua���л���, int ջ����)

//��ָ��ջ������ջ��Ԫ��ת��Ϊ����
//lua_tointeger(lua_State* lua���л���, int ջ����)

//��ָ��ջ������ջ��Ԫ��ת��Ϊ�ַ���
//lua_tostring(lua_State* lua���л���, int ջ����)

//�õ�lua�����е�ȫ�ֱ�ʶ�����������lua���л�����ջ��
//lua_getglobal(lua_State* lua���л���, const char* lua�����е�ȫ�ֱ�ʶ��)

//����ǰlua���л�����ջ��Ԫ��ȡһ����ʶ�����ֲ����õ�lua������
//lua_setglobal(lua_State* lua���л���, const char* lua�����е�ȫ�ֱ�ʶ��)

//����lua����
//lua_pcall(lua_State* lua���л���, int ��������, int ����ֵ����, int 0)

//����lua����һ��f��������
//function f(a,b)
//	return a + b, a - b, a * b, a / b
//end

//�������Ҫ�������f���������õ�3������ֵ�Ļ�����
//���뽫lua���л����е�ջ����Ϊ��������ӣ�

//[���ݸ�b������]��
//[���ݸ�a������]
//[f����]
//[...]
//[...]��

//Ȼ����ô���
//lua_pcall(lua���л���, 2, 2, 0)

//����������ĺ���֮��lua�Զ����f������3������ֵ��
//�뵽lua���л����е�ջ����
//[a*b]��
//[a-b]
//[a+b]
//[...]
//[...]��

