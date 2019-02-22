a = 1

function b(x, y)
	return x + y
end

local c = 2

local function d(x, y, z)
	return x + y + z
end

local function e()
	local g = 1
	local function f()
		g = g + 1
		return g
	end
	return f
end

h = e()
print(h())
print(h())
print(h())

local function i()
	local g = {}
	g.a = 123
	g.b = 456
	g.c = 789
	return g
end

h = i()
print(h.a)
print(h.b)
print(h.c)
