--单行注释

--[[多行注释
print(123)
print(456)
print(789)
--]]

--print函数的作用是打印
--type函数的作用是得到指定标识符的类型字符串

--[[基本数据类型
空类型：nil，类似C语言中的NULL
布尔类型：true、false
数值类型：lua的数值类型内部就是用的C语言的double
字符串类型：""、''和两对中括号
表类型：初看有点像数组，但实际上类似c++中的map
函数类型：可以直接用标识符去被函数赋值
用户类型（使用不多）：主要用于C、Lua之间的交互
线程类型（使用不多）：Lua也支持多线程程序
--]]

a = nil
b = true
c = 3.14
d = "abcdef"
e = 'xyz'
f = [[你好世界]]
g = {}
g[1] = 100
g[2] = 101
g[3] = 104
h = print

--[[
print(a)
print(b)
print(c)
print(d)
print(e)
print(f)
print(g[1])
print(g[2])
print(g[3])
print(h)
--]]

--[[
h(a)
h(b)
h(c)
h(d)
h(e)
h(f)
h(g[1])
h(g[2])
h(g[3])
h(h)
=]]

h(type(a))
h(type(b))
h(type(c))
h(type(d))
h(type(e))
h(type(f))
h(type(g[1]))
h(type(g[2]))
h(type(g[3]))
h(type(h))
h(type(type(h)))

--如果之前没有x，则Lua生成1个全局变量x并用
--数值1进行赋值
x = 1

--目前已经有了x，所以Lua就用"123abc"来替换
--这个x原来存储的数值类型数据1
x = "123abc"

--生成y，然后用bool类型true赋值
y = true

--用y保存的bool类型数据替换x保存的字符串类型
--数据"123abc"
x = y

--之前没有z，此处就会产生全局变量z然后用
--x保存的bool类型数据赋值给z
z = x

print(x)
print(y)
print(z)
print(type(x))
print(type(y))
print(type(z))
