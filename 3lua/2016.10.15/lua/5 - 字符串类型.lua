--[[
lua中字符串可以用三种方式来构造，可以使用..
来进行字符串连接，也可以用#来得到字符串的长度
--]]

a = "abcdef你好" --双引号
b = 'xyz世界' --单引号
c = [[uio积极]] --中括号

print(a)
print(b)
print(c)

d = a .. b .. "hello"
print(d)

--整数可以和字符串进行连接，在连接
--的时候lua会把数值转换成字符串再进
--行连接
e = c .. 3.14
print(e)

--f = c .. false

print(#"abc你好")
print(#e)


