a = {}
a[1] = 100
a[2] = 200
a["abc"] = 300
a[true] = 500
a[4] = 400
a[7] = 600
a[3] = 800

--��ֵѭ���޷������������������
--�п�λ����û��ʹ�õ������±꣬��
--�������������͵��±�
for i = 1, #a do
	print(a[i])
end

print("--------------------")

--����Ĵ�����Եõ�a���е�ÿ����ֵ��
for index, data in pairs(a) do
	print(tostring(index) .. " - " .. tostring(data))
end

print("--------------------")

--����Ĵ���ֻ���ڵõ��±�
for index in pairs(a) do
	print(tostring(index))
end

print("--------------------")

--ʹ��ipairs��ѭ����ֻ�ܵõ������±��Ӧ
--�����м�ֵ�ԣ�����������п�λ�ĵط���
--����ѭ�������������a����[5]�ǿ�λ����
--ѭ����4���˳�
for index, data in ipairs(a) do
	print(tostring(index) .. " - " .. tostring(data))
end
