a = {}

a.y = 1
a.z = 2

a.f =
function (t, x)
	print(t.y + t.z + x)
end

--这种用法中t有点类似C++中的this指
--针，因为传递的是自己这个表
a.f(a, 3)
