function f1(...)
	--如果是不定参函数，则可以通过
	--下面的代码得到所有的参数，这
	--些参数被放一个表中赋值给a
	local a = {...}
	for k, d in pairs(a) do
		print(k .. "-" .. tostring(d))
	end
end

f1(100, 200, "abc", 300, false)
--上面调用中，会传入表{100,200,"abc",300,false}

function get_add(...)
	local a = {...}
	local b = 0
	for i = 1, #a do
		b = b + a[i]
	end
	return b
end

print(get_add(1, 2, 3, 4))
print(get_add(1, 2, 3, 4, 5, 6, 7))
