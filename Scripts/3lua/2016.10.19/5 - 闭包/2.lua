function f1()
	local num = 0
	local function f2()
		num = num + 1
		return num
	end
	return f2
end

a = f1()
print(a())
print(a())
print(a())

b = a
print(b())
print(b())
print(b())
print(b())
