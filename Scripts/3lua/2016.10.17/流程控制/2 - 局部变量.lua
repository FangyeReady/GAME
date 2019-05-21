--如果没有用local来修饰一个标识符，那么
--该标识符就表示一个全局变量，只有用local
--local修饰的标识符那么才是局部变量，局部
--变量在程序退出其作用之后就没有了，一个
--作用域是以end作为结束的

a = 1 --全局变量
local b = 2 --局部变量：整个文件
if true then
	c = 3 --全局变量
	local d = 4 --局部变量：if-end之间
	print(d)
end
print(a)
print(b)
print(c)
print(d)

--for到do之间的循环变量默认就是局部变量，退出
--循环之后就没有了
for i = 1, 5 do
	j = i
end

print(i)
print(j)
