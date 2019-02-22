--[[
lua支持多赋值
--]]

a, b = 1, 2
print(a)
print(b)

d, e, f = 3, 4
print(d)
print(e)
print(f)

g, h = 5, 6, 7
print(g)
print(h)

x = 100
y = 200
x, y = y, x
print(x)
print(y)
--有了多赋值，那么可以用上述代码来完成两个值的交换
