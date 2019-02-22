function add2(a, b)
	return a + b
end

x = add2(1, 2)
print(x)

--lua中函数定义的本质是下面的代码
--上面的add2函数的定义会被处理成
--类似下面add3的定义，lua中的函数
--更像一个变量，所以是定义在其它函
--数中的；下面代码中就是生成了一堆
--操作指令，然后为这一堆操作指令取
--名为add3
add3 = function (a, b, c)
	return a + b + c
end



--lua中，可以用#表得到这个表
--中最大的整数下标
y = {"ab", "cd", "ef"}
y[4] = "gh"
y[6] = "xy"
print(#y)

print("--------------------------")

sort_bubble = function (t)
	for i = #t - 1, 1, -1 do
		for j = 1, i do
			if t[j] > t[j + 1] then
				t[j], t[j + 1] = t[j + 1], t[j]
			end
		end
	end
end

w = {5, 6, 4, 7, 9, 0, 8, 3, 1, 2}
sort_bubble(w)

for i = 1, #w do
	print(w[i])
end







