a = {}
a[1] = 100
a[2] = 200
a["abc"] = 300
a[true] = 500
a[4] = 400
a[7] = 600
a[3] = 800

--数值循环无法遍历整个表，如果表中
--有空位，即没有使用的整数下标，或
--表中有其它类型的下标
for i = 1, #a do
	print(a[i])
end

print("--------------------")

--下面的代码可以得到a表中的每个键值对
for index, data in pairs(a) do
	print(tostring(index) .. " - " .. tostring(data))
end

print("--------------------")

--下面的代码只用于得到下标
for index in pairs(a) do
	print(tostring(index))
end

print("--------------------")

--使用ipairs的循环，只能得到整数下标对应
--的所有键值对，且如果遇到有空位的地方就
--跳出循环，比如上面的a表在[5]是空位，所
--循环到4就退出
for index, data in ipairs(a) do
	print(tostring(index) .. " - " .. tostring(data))
end
