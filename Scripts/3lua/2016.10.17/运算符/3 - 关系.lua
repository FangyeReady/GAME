--[[
��ϵ��������Ƿ���true��false
==������ж������
~=�������ж������
>�������ж������
>=�����ڵ����ж������
<��С���ж������
<=��С�ڵ����ж������
--]]

--������Ͳ�ͬ����==һ�����ؼ٣���~=һ��������
a = true
b = 123
if a == b then
	print(1)
end
if c ~= b then
	print(2)
end

--���������ͬ������ѭ��������㷨��

--nilֻ�ܺ�nil�����
if x == y then
	print(3)
end

--bool���ͺ���ֵ���ͽ��������ж��Ƿ���ͬ
x = false
if x == false then
	print(4)
end
x = 123
y = 123.0001
if x == y then
	print(5)
end

--�ַ������Ͱ����ֵ�ȽϷ�����������ж�
x = "nihao"
y = "NiHao"
if x ~= y then
	print(6)
end

--�����Ͱ����Ƿ�ָ��ͬһ�ű��������Ƿ������
x = {} --����һ�ű���xָ��
y = {} --����һ�ű���yָ��
if x == y then
	print(7)
end
z = x --��xָ��ı�Ҳ��ֵ��zָ��
if x == z then
	print(8)
end

--��������Ҳ�ǰ����Ƿ�ָ��ͬһ�����������Ƿ����
function x()
end
y = x
if x == y then
	print(9)
end

function f1()
	function f2()
		return 1
	end
	return f2
end

x = f1
y = x
if x == y then
	print(10)
end
print(x)
print(y)

x = f1
y = f1
if x == y then
	print(11)
end
print(x)
print(y)

--[[
x = f1()
y = f1()
if x == y then
	print(12)
end
print(x)
print(y)
lua��û�к�����ֻ�бհ���lua�еġ���������C�е��кܴ�ͬ
--]]

--[[
>��>=��<��<=�������
nil��bool�������������ܽ�����������
��ֵ�Ļ�ֱ�Ӱ�����ֵ��С���бȽ�
�ַ����Ļ������ֵ�ȽϷ����бȽ�
--]]
a = 123
b = 321
if a >= b then
	print(13)
end

a = "abcdef"
b = "as"
if a < b then
	print(14)
end





