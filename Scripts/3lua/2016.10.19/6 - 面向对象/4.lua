a = {}

a.y = 1
a.z = 2

function a:f(x)
	print(self.y + self.z + x)
end

--上面在定义函数f的时候，不是用的.而是
--用的:，用:定义出来的函数默认有一个参数
--叫做self，哪个具体的表在调用f函数，那么
--self会自动被赋值为哪个表的，等于于this

a:f(3)

--上面的f函数也可以写为如下的情况

a.ff = function (self, x)
	print(self.y + self.z + x)
end

a:ff(3)
