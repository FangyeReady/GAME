--表类型，表类型是lua中直接提供的一种数据
--结构，行为模式上有点类似std::map，即通过
--“键”去访问“值”，在lua中，任何数据类型都可
--以当做“键”或“值”，如果我们用整数作为键，
--相当于就是C++中的数组，如果用其它类型作为键
--则用法很类似std::map<类型1,类型2>
a = {}
a[1] = 123
a[2] = 456
a[3] = 789
print(a[1])
print(a[2])
print(a[3])
a[true] = "hello"
print(a[true])
a[true] = a[2]
print(a[true])
a[3.14] = a[3]
print(a[3.14])
a["xyz"] = false
print(a["xyz"])
print(a[5]) --a[5]没有对应数值就是nil
--如果我们用字符串去作键，那么还可以简化
--如下，即字符串作为键，可以直接用表名.
--进行访问，其它类型都不允许这样
a["def"] = 100
print(a.def)
a.xyz = 321
print(a["xyz"])
a[a.xyz] = 789 --a[321] = 789
print(a[321])

b = {}
b.x = {} --b["x"] = {}
b["y"] = 123 --b["y"] = 123
b.x[b.y] = 456 --b["x"]["y"] = 456
			   --b.x.y = 456
print(b.x[b.y])
print(b.x.y) --b["x"]["y"]

for i = 1, 5 do
	b[i] = tonumber(io.read());
end

for i = 1, 5 do
	print("第" .. i .. "个变量为" .. b[i])
end

maxnum = b[1]
for i = 2, 5 do
	if maxnum < b[i] then
		maxnum = b[i]
	end
end
print(maxnum)

--lua中如果把表作为C语言中的数组来使用
--则用1作为下标的开始，而不是C语言中的
--0了

