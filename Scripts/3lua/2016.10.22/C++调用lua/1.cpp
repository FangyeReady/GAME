#include <iostream>
#include "src/lua.hpp"

//lua���е�����ͷ�ļ���Դ�ļ�����������֮����Ҫ��
//lua.c��luac.c�е�����main����ע���ˣ�����������
//�Լ������е�main����������ͻ

int f(lua_State* ls)
{
	return 0;
}

void test1()
{
	lua_State* ls = luaL_newstate();
	//[�ס���]

	lua_pushinteger(ls, 100);
	//[100]������

	lua_pushinteger(ls, 200);
	//[200]��
	//[100]��

	lua_pushnumber(ls, 3.14);
	//[3.14]��
	//[200]
	//[100]��
	
	lua_pushnumber(ls, 5.12);
	//[5.12]��
	//[3.14]
	//[200]
	//[100]��

	lua_pushstring(ls, "123abc");
	//["123abc"]��
	//[5.12]
	//[3.14]
	//[200]
	//[100]��

	lua_pushstring(ls, "���");
	//["���"]��
	//["123abc"]
	//[5.12]
	//[3.14]
	//[200]
	//[100]��

	lua_settop(ls, 3);
	//[3.14]��
	//[200]
	//[100]��

	lua_pushcfunction(ls, f);
	//[f����]��
	//[3.14]
	//[200]
	//[100]��

	std::cout<<"��ǰջ��Ԫ��������"<<lua_gettop(ls)<<std::endl;

	std::cout<<"-2��"<<lua_tonumber(ls, -2)<<std::endl;
	std::cout<<"-3��"<<lua_tointeger(ls, -3)<<std::endl;
	std::cout<<"-4��"<<lua_tointeger(ls, -4)<<std::endl;

	lua_close(ls);
}

void test2()
{
	lua_State* ls = luaL_newstate();

	//����lua�ļ�
	luaL_loadfile(ls, "test2.lua");
	//[test2.lua��Ϊlua����]�ס���

	//ִ��lua����������lua�����е����ж�������������
	lua_pcall(ls, 0, 0, 0);
	//[]�ס���

	lua_getglobal(ls, "a");
	lua_getglobal(ls, "b");
	lua_getglobal(ls, "c");
	//[c]��
	//[b]
	//[a]��

	std::cout<<lua_tostring(ls, -1)<<std::endl;
	std::cout<<lua_tonumber(ls, -2)<<std::endl;
	std::cout<<lua_tointeger(ls, -3)<<std::endl;

	lua_settop(ls, 0);
	//[]������
	
	lua_getglobal(ls, "d");
	//[d����]������

	lua_pushnumber(ls, 5);
	lua_pushnumber(ls, 9);
	//[9]��
	//[5]
	//[d����]��

	//����lua�����еģ�d(5,9)
	lua_pcall(ls, 2, 4, 0);
	//[0.55555]��
	//[45]��
	//[-4]
	//[14]��

	std::cout<<"��ǰջ��Ԫ��������"<<lua_gettop(ls)<<std::endl;
	std::cout<<lua_tonumber(ls, -1)<<std::endl;
	std::cout<<lua_tonumber(ls, -2)<<std::endl;
	std::cout<<lua_tonumber(ls, -3)<<std::endl;
	std::cout<<lua_tonumber(ls, -4)<<std::endl;

	//�൱���������ջ
	lua_settop(ls, 0);
	//[]������

	lua_getglobal(ls, "e");
	//[e����]������
	lua_pushnumber(ls, 3.2);
	lua_pushnumber(ls, 4.1);
	lua_pushnumber(ls, 2.7);
	//[2.7]��
	//[4.1]��
	//[3.2]
	//[e����]��

	lua_pcall(ls, 3, 1, 0);
	//[4.1]������

	std::cout<<lua_tonumber(ls, -1)<<std::endl;

	lua_close(ls);
}

void main()
{
	test2();

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

