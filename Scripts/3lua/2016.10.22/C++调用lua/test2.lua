a = 100
b = 3.14
c = "hello world"
function d(a,b)
	return a + b, a - b, a * b, a / b
end

function e(a,b,c)
	local f
	if a > b then
		f = a
	else
		f = b
	end
	if f > c then
		return f
	else
		return c
	end
end
