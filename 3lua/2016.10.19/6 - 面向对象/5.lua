--下面的函数创建学生类对象

function create_stu(ID, NAME, AGE)

	--成员变量
	local stu = {id = ID, name = NAME, age = AGE}

	--成员函数
	function stu:set_id(ID)
			self.id = ID
	end

	function stu:get_id()
			return self.id
	end

	function stu:set_name(NAME)
			self.name = NAME
	end

	function stu:get_name()
			return self.name
	end

	function stu:set_age(AGE)
			self.age = AGE
	end

	function stu:get_age()
			return self.age
	end

	return stu
end

stu1 = create_stu(123, "abc", 21)

stu1:set_id(234) -- 等同于stu1.set_id(stu1, 234)
stu1:set_name("def")
stu1:set_age(32)

print(stu1:get_id())
print(stu1:get_name())
print(stu1:get_age())

stu2 = create_stu(123, "abc", 21)

print(stu2:get_id())
print(stu2:get_name())
print(stu2:get_age())











