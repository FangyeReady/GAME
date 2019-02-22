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

function create_new_stu(ID, NAME, AGE, BLOOD)

	--创建老的对象
	new_stu = create_stu(ID, NAME, AGE)

	--添加新的数据
	new_stu.blood = BLOOD

	--添加新的函数

	function new_stu:set_blood(BLOOD)
			self.blood = BLOOD
	end

	function new_stu:get_blood()
			return self.blood
	end

	return new_stu
end

stu1 = create_new_stu(123, "abc", 21, "AB")
print(stu1:get_id())
print(stu1:get_blood())

stu1:set_blood("O")
print(stu1:get_blood())

--多态
function create_new_stu2(ID, NAME, AGE, BLOOD)

	--创建老的对象
	new_stu = create_new_stu(ID, NAME, AGE, BLOOD)

	--替换函数

	function new_stu:set_blood(BLOOD)
			self.blood = BLOOD .. "血型"
	end

	function new_stu:get_blood()
			return self.blood
	end

	return new_stu
end

stu2 = create_new_stu2(123, "abc", 21, "AB")
stu2:set_blood("A")
print(stu2:get_blood())





