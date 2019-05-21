--等待用户从键盘上面输入一个字符串并用回车
--作为结束，然后把字符串作为函数的参数返回
print("请输入一个数值")
a = io.read()

--将字符串转换为数值类型，如果转换成功则返回
--数值，转换失败则返回nil

b = tonumber(a) --atoi
if b == nil then
	print("转换失败")
end

print("请再输入一个数值")
c = tonumber(io.read())

d = b + c

print(b .. " + " .. c .. " = " .. d)

