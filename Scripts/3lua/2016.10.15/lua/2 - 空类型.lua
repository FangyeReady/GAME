--�κα�ʶ���ĳ��ֶ��ᵼ��lua�������
--��ʶ������Ĭ�ϱ�ʶ���洢�����;���nil
--���ͣ����������Լ�������ֵΪ��������

--��lua��һ����ʶ������ǿ����ͣ���ô��
--�е�����C�����еĿ�ָ��

--��lua�У��߼��жϵ�����������£�nil��
--false��ʾ�٣������κζ�������ʾ�棬����0

print(a)

b = a
print(b)

b = false
print(b)
print(type(b))

if nil then
	print("!!!!!!")
end

if false then
	print("??????")
end

if 0 then
	print("######")
end

if print then
	print("******")
end
