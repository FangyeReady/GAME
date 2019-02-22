--opr2函数可以一次返回4个计算结果
function opr2(a, b)
	return a + b, a - b, a * b, a / b
end

x1, y1, z1, w1 = opr2(1, 2)
print(x1)
print(y1)
print(z1)
print(w1)

print("--------------------------")

x2, y2 = opr2(1, 2)
print(x2)
print(y2)

print("--------------------------")

x3 = opr2(1, 2)
print(x3)

print("--------------------------")

x4, y4, z4, w4, q4 = opr2(1, 2)
print(x4)
print(y4)
print(z4)
print(w4)
print(q4)

print("--------------------------")

x5, y5, z5, w5, q5 = 100, opr2(1, 2)
print(x5)
print(y5)
print(z5)
print(w5)
print(q5)

print("--------------------------")

x6, y6, z6, w6, q6 = opr2(1, 2), 100
print(x6)
print(y6)
print(z6)
print(w6)
print(q6)

--如果函数作为调用不是整个赋值表达式
--的最后一个表达式或者唯一的表达式，
--则只会用到函数的第一个返回值结果，
--如果函数是最后一个表达式或者唯一的
--一个表达式则它的结果会尽量从左往右
--为接收结果的标识符进行赋值

print("--------------------------")

function f()
	return 1, 2, 3
end

a1, a2, a3, a4, a5, a6 = f(), 1, f();

print(a1)
print(a2)
print(a3)
print(a4)
print(a5)
print(a6)
