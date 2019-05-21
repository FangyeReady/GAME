--任何标识符的出现都会导致lua产生这个
--标识符，且默认标识符存储的类型就是nil
--类型，除非我们自己给它赋值为其它类型

--在lua中一个标识符如果是空类型，那么就
--有点类似C语言中的空指针

--在lua中，逻辑判断的真假区别如下，nil和
--false表示假，其它任何东西都表示真，包括0

print(a)

b = a
print(b)

b = false
print(b)
print(type(b))

if nil then
	print("!!!!!!")
end

if false then
	print("??????")
end

if 0 then
	print("######")
end

if print then
	print("******")
end
