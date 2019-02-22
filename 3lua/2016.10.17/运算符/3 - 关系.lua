--[[
关系运算符总是凡是true或false
==：相等判断运算符
~=：不等判断运算符
>：大于判断运算符
>=：大于等于判断运算符
<：小于判断运算符
<=：小于等于判断运算符
--]]

--如果类型不同，则==一定返回假，而~=一定返回真
a = true
b = 123
if a == b then
	print(1)
end
if c ~= b then
	print(2)
end

--如果类型相同，则遵循下面的运算法则

--nil只能和nil算相等
if x == y then
	print(3)
end

--bool类型和数值类型进行数据判断是否相同
x = false
if x == false then
	print(4)
end
x = 123
y = 123.0001
if x == y then
	print(5)
end

--字符串类型按照字典比较法来进行相等判断
x = "nihao"
y = "NiHao"
if x ~= y then
	print(6)
end

--表类型按照是否指向同一张表来决定是否算相等
x = {} --创建一张表并让x指向
y = {} --创建一张表并让y指向
if x == y then
	print(7)
end
z = x --把x指向的表也赋值给z指向
if x == z then
	print(8)
end

--函数类型也是按照是否指向同一个函数来算是否相等
function x()
end
y = x
if x == y then
	print(9)
end

function f1()
	function f2()
		return 1
	end
	return f2
end

x = f1
y = x
if x == y then
	print(10)
end
print(x)
print(y)

x = f1
y = f1
if x == y then
	print(11)
end
print(x)
print(y)

--[[
x = f1()
y = f1()
if x == y then
	print(12)
end
print(x)
print(y)
lua中没有函数，只有闭包，lua中的“函数”和C中的有很大不同
--]]

--[[
>、>=、<、<=运算符中
nil、bool、表、函数都不能进行上述运算
数值的话直接按照数值大小进行比较
字符串的话按照字典比较法进行比较
--]]
a = 123
b = 321
if a >= b then
	print(13)
end

a = "abcdef"
b = "as"
if a < b then
	print(14)
end





