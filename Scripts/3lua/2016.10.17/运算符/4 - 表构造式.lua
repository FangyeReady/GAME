a = {true, false, "123abc", 123}
--[[
a = {}
a[1] = true
a[2] = false
a[3] = "123abc"
a[4] = 123
--]]
print(a[1])
print(a[2])
print(a[3])
print(a[4])

b = {ab = true, cd = false, xyz = "123abc"}
--[[
b = {}
b["ab"] = true
b["cd"] = false
b["xyz"] = "123abc"
--]]
print(b.ab)
print(b.cd)
print(b.xyz)

c = {"fff", q = "ttt", 123, p = 12345}
--[[
c = {}
c[1] = "fff"
c["q"] = "ttt"
c[2] = 123
c["p"] = 12345
--]]
print(c[1])
print(c.q)
print(c[2])
print(c.p)

student = {id = 12345, name = "abc", age = 22}
print(student.id)
print(student.name)
print(student.age)

--[[
head = {}
p = head
i = 1
while i <= 5 do
	p[1] = 100 + i
	p[2] = {}
	p = p[2]
	i = i + 1
end
p = head
while p ~= nil do
	print(p[1])
	p = p[2]
end
--]]

head = {}
p = head
i = 1
while i <= 5 do
	p["data"] = 100 + i
	p["next"] = {}
	p = p.next
	i = i + 1
end
p = head
while p ~= nil do
	print(p.data)
	p = p.next
end

