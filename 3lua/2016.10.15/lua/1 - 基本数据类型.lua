--����ע��

--[[����ע��
print(123)
print(456)
print(789)
--]]

--print�����������Ǵ�ӡ
--type�����������ǵõ�ָ����ʶ���������ַ���

--[[������������
�����ͣ�nil������C�����е�NULL
�������ͣ�true��false
��ֵ���ͣ�lua����ֵ�����ڲ������õ�C���Ե�double
�ַ������ͣ�""��''������������
�����ͣ������е������飬��ʵ��������c++�е�map
�������ͣ�����ֱ���ñ�ʶ��ȥ��������ֵ
�û����ͣ�ʹ�ò��ࣩ����Ҫ����C��Lua֮��Ľ���
�߳����ͣ�ʹ�ò��ࣩ��LuaҲ֧�ֶ��̳߳���
--]]

a = nil
b = true
c = 3.14
d = "abcdef"
e = 'xyz'
f = [[�������]]
g = {}
g[1] = 100
g[2] = 101
g[3] = 104
h = print

--[[
print(a)
print(b)
print(c)
print(d)
print(e)
print(f)
print(g[1])
print(g[2])
print(g[3])
print(h)
--]]

--[[
h(a)
h(b)
h(c)
h(d)
h(e)
h(f)
h(g[1])
h(g[2])
h(g[3])
h(h)
=]]

h(type(a))
h(type(b))
h(type(c))
h(type(d))
h(type(e))
h(type(f))
h(type(g[1]))
h(type(g[2]))
h(type(g[3]))
h(type(h))
h(type(type(h)))

--���֮ǰû��x����Lua����1��ȫ�ֱ���x����
--��ֵ1���и�ֵ
x = 1

--Ŀǰ�Ѿ�����x������Lua����"123abc"���滻
--���xԭ���洢����ֵ��������1
x = "123abc"

--����y��Ȼ����bool����true��ֵ
y = true

--��y�����bool���������滻x������ַ�������
--����"123abc"
x = y

--֮ǰû��z���˴��ͻ����ȫ�ֱ���zȻ����
--x�����bool�������ݸ�ֵ��z
z = x

print(x)
print(y)
print(z)
print(type(x))
print(type(y))
print(type(z))
