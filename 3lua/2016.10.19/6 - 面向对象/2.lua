a = {}

a.y = 1
a.z = 2

a.f =
function (t, x)
	print(t.y + t.z + x)
end

--�����÷���t�е�����C++�е�thisָ
--�룬��Ϊ���ݵ����Լ������
a.f(a, 3)
