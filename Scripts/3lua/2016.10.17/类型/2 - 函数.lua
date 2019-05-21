--lua中函数无返回值类型，用funcition
--关键字来表示定义的是函数，函数以
--function ()作为开始，以end作为结束

function abc(a,b)
	print("hello")
	return a + b
end

--[[lua会把上面的函数处理为下面的样子
abc =
	function (a,b)
		print("hello")
		return a + b
	end
--]]

--函数调用
x = abc(1,2)
print(x)

--函数赋值给标识符
y = abc
print(y(2,3))

def =
	function (a,b,c)
		return a + b + c
	end

print(def(1, 2, 3))

y = {}
y[1] =
	function (a,b,c)
		return a * b * c
	end
print(y[1](2, 3, 4))

--用户数据、线程在一般的游戏开发中使用不多
