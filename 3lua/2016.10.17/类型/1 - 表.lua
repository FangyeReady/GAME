--�����ͣ���������lua��ֱ���ṩ��һ������
--�ṹ����Ϊģʽ���е�����std::map����ͨ��
--������ȥ���ʡ�ֵ������lua�У��κ��������Ͷ���
--�Ե�����������ֵ�������������������Ϊ����
--�൱�ھ���C++�е����飬���������������Ϊ��
--���÷�������std::map<����1,����2>
a = {}
a[1] = 123
a[2] = 456
a[3] = 789
print(a[1])
print(a[2])
print(a[3])
a[true] = "hello"
print(a[true])
a[true] = a[2]
print(a[true])
a[3.14] = a[3]
print(a[3.14])
a["xyz"] = false
print(a["xyz"])
print(a[5]) --a[5]û�ж�Ӧ��ֵ����nil
--����������ַ���ȥ��������ô�����Լ�
--���£����ַ�����Ϊ��������ֱ���ñ���.
--���з��ʣ��������Ͷ�����������
a["def"] = 100
print(a.def)
a.xyz = 321
print(a["xyz"])
a[a.xyz] = 789 --a[321] = 789
print(a[321])

b = {}
b.x = {} --b["x"] = {}
b["y"] = 123 --b["y"] = 123
b.x[b.y] = 456 --b["x"]["y"] = 456
			   --b.x.y = 456
print(b.x[b.y])
print(b.x.y) --b["x"]["y"]

for i = 1, 5 do
	b[i] = tonumber(io.read());
end

for i = 1, 5 do
	print("��" .. i .. "������Ϊ" .. b[i])
end

maxnum = b[1]
for i = 2, 5 do
	if maxnum < b[i] then
		maxnum = b[i]
	end
end
print(maxnum)

--lua������ѱ���ΪC�����е�������ʹ��
--����1��Ϊ�±�Ŀ�ʼ��������C�����е�
--0��

