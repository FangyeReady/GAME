--lua�к����޷���ֵ���ͣ���funcition
--�ؼ�������ʾ������Ǻ�����������
--function ()��Ϊ��ʼ����end��Ϊ����

function abc(a,b)
	print("hello")
	return a + b
end

--[[lua�������ĺ�������Ϊ���������
abc =
	function (a,b)
		print("hello")
		return a + b
	end
--]]

--��������
x = abc(1,2)
print(x)

--������ֵ����ʶ��
y = abc
print(y(2,3))

def =
	function (a,b,c)
		return a + b + c
	end

print(def(1, 2, 3))

y = {}
y[1] =
	function (a,b,c)
		return a * b * c
	end
print(y[1](2, 3, 4))

--�û����ݡ��߳���һ�����Ϸ������ʹ�ò���
