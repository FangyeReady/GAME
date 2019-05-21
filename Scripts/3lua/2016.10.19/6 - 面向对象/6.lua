--����ĺ�������ѧ�������

function create_stu(ID, NAME, AGE)

	--��Ա����
	local stu = {id = ID, name = NAME, age = AGE}

	--��Ա����
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

	--�����ϵĶ���
	new_stu = create_stu(ID, NAME, AGE)

	--����µ�����
	new_stu.blood = BLOOD

	--����µĺ���

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

--��̬
function create_new_stu2(ID, NAME, AGE, BLOOD)

	--�����ϵĶ���
	new_stu = create_new_stu(ID, NAME, AGE, BLOOD)

	--�滻����

	function new_stu:set_blood(BLOOD)
			self.blood = BLOOD .. "Ѫ��"
	end

	function new_stu:get_blood()
			return self.blood
	end

	return new_stu
end

stu2 = create_new_stu2(123, "abc", 21, "AB")
stu2:set_blood("A")
print(stu2:get_blood())





