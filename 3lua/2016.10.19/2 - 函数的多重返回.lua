--opr2��������һ�η���4��������
function opr2(a, b)
	return a + b, a - b, a * b, a / b
end

x1, y1, z1, w1 = opr2(1, 2)
print(x1)
print(y1)
print(z1)
print(w1)

print("--------------------------")

x2, y2 = opr2(1, 2)
print(x2)
print(y2)

print("--------------------------")

x3 = opr2(1, 2)
print(x3)

print("--------------------------")

x4, y4, z4, w4, q4 = opr2(1, 2)
print(x4)
print(y4)
print(z4)
print(w4)
print(q4)

print("--------------------------")

x5, y5, z5, w5, q5 = 100, opr2(1, 2)
print(x5)
print(y5)
print(z5)
print(w5)
print(q5)

print("--------------------------")

x6, y6, z6, w6, q6 = opr2(1, 2), 100
print(x6)
print(y6)
print(z6)
print(w6)
print(q6)

--���������Ϊ���ò���������ֵ���ʽ
--�����һ�����ʽ����Ψһ�ı��ʽ��
--��ֻ���õ������ĵ�һ������ֵ�����
--������������һ�����ʽ����Ψһ��
--һ�����ʽ�����Ľ���ᾡ����������
--Ϊ���ս���ı�ʶ�����и�ֵ

print("--------------------------")

function f()
	return 1, 2, 3
end

a1, a2, a3, a4, a5, a6 = f(), 1, f();

print(a1)
print(a2)
print(a3)
print(a4)
print(a5)
print(a6)
