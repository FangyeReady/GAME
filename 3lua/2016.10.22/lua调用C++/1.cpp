#include <iostream>
#include "src/lua.hpp"

//lua要求调用的C语言函数的样子必须是返回值为int，形式
//参数表为一个lua_State指针，lua在调用注册的C语言函数
//的时候，会把栈清空，然后把调用的参数依次放入到栈中，
//我们在C语言函数中就可以得到栈中的多个参数数据，最后
//我们C语言处理完了，如果有处理结果要传递给lua的话，就
//需要通过返回值告知lua我们放了多少个数据在栈中，lua调
//用的时候就可以通过多重返回值得到这些多个数据了

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
	//[dy函数]底、顶

	lua_setglobal(ls, "c_dy");
	//[]底、顶
	//在lua的全局域中就有了一个叫做c_dy的函数

	lua_pushcfunction(ls, dy_i);
	lua_setglobal(ls, "c_dy_i");
	lua_pushcfunction(ls, cl);
	lua_setglobal(ls, "c_cl");

	luaL_loadfile(ls, "test2.lua");
	lua_pcall(ls, 0, 0, 0);

	lua_close(ls);

	system("pause");
}

//创建lua运行环境
//luaL_newstate()

//装载lua文件并检查语法错误，装载完毕之后，整个lua文件
//就相当于一个大的、无名的lua函数被放在当前的lua运行环
//境的栈顶
//luaL_loadfile(lua_State* lua运行环境, char* lua文件)

//装载包含lua源代码的字符串并检查语法错误
//luaL_loadstring(lua_State* lua运行环境, char* 包含lua源代码的字符串)

//关闭lua运行环境
//lua_close(lua_State* lua运行环境)

//lua与c++的交互，主要依靠一个栈来完成，这个
//栈相比一般意义上的栈而言更像是一个顺序表，
//从创建lua运行环境之初这个栈也就被创建出来了
//，当然是放在lua运行环境中的，对于这个栈而言
//可以使用正整数或负整数来对栈中元素进行访问，
//访问的规则如下所示：
//+4[顶]-1
//+3[..]-2
//+2[..]-3
//+1[底]-4
//如果我们以栈顶为基准开始访问栈中元素，则以
//负数作为下标，-1就是栈顶，-2就是栈顶下面的
//一个元素，以此类推；如果我们以栈底为基准开
//始访问栈中元素，则以正数作为下标，1就是栈底
//，2就是栈底上面一个元素，以此类推。

//得到当前lua运行环境中栈顶距离栈底的长度
//lua_gettop(lua_State* lua运行环境)

//设置当前栈顶的位置为num，比如当前栈顶本
//身距离栈底有5个元素，如果调用下面的代码
//为lua_settop(ls, 3)则会把5个元素中的最上
//面两个元素弹开，在栈中就只剩下3个元素
//lua_settop(lua_State* lua运行环境, int num)

//推入一个整数到lua运行环境的栈顶
//lua_pushinteger(lua_State* lua运行环境, int i)

//推入一个浮点数到lua运行环境的栈顶
//lua_pushnumber(lua_State* lua运行环境, double d)

//推入一个字符串到lua运行环境的栈顶
//lua_pushstring(lua_State* lua运行环境, const char* str)

//推入一个C语言函数到lua运行环境的栈顶，注意该C语言函数的声明如下
//int 函数名(lua_State* lua运行环境)
//lua_pushcfunction(lua_State* lua运行环境, int (*f)(lua_State*))

//将指定栈索引的栈中元素转换为浮点数
//lua_tonumber(lua_State* lua运行环境, int 栈索引)

//将指定栈索引的栈中元素转换为整数
//lua_tointeger(lua_State* lua运行环境, int 栈索引)

//将指定栈索引的栈中元素转换为字符串
//lua_tostring(lua_State* lua运行环境, int 栈索引)

//得到lua代码中的全局标识符并将其放入lua运行环境的栈顶
//lua_getglobal(lua_State* lua运行环境, const char* lua代码中的全局标识符)

//将当前lua运行环境的栈顶元素取一个标识符名字并设置到lua代码中
//lua_setglobal(lua_State* lua运行环境, const char* lua代码中的全局标识符)

//调用lua函数
//lua_pcall(lua_State* lua运行环境, int 参数个数, int 返回值个数, int 0)

//假设lua中有一个f函数如下
//function f(a,b)
//	return a + b, a - b, a * b, a / b
//end

//我们如果要调用这个f函数，并得到3个返回值的话，则
//必须将lua运行环境中的栈设置为下面的样子：

//[传递给b的数据]顶
//[传递给a的数据]
//[f函数]
//[...]
//[...]底

//然后调用代码
//lua_pcall(lua运行环境, 2, 2, 0)

//调用完上面的函数之后，lua自动会把f函数的3个返回值放
//入到lua运行环境中的栈里面
//[a*b]顶
//[a-b]
//[a+b]
//[...]
//[...]底

