function add2(a, b)
	return a + b
end

x = add2(1, 2)
print(x)

--lua�к�������ı���������Ĵ���
--�����add2�����Ķ���ᱻ�����
--��������add3�Ķ��壬lua�еĺ���
--����һ�������������Ƕ�����������
--���еģ���������о���������һ��
--����ָ�Ȼ��Ϊ��һ�Ѳ���ָ��ȡ
--��Ϊadd3
add3 = function (a, b, c)
	return a + b + c
end



--lua�У�������#��õ������
--�����������±�
y = {"ab", "cd", "ef"}
y[4] = "gh"
y[6] = "xy"
print(#y)

print("--------------------------")

sort_bubble = function (t)
	for i = #t - 1, 1, -1 do
		for j = 1, i do
			if t[j] > t[j + 1] then
				t[j], t[j + 1] = t[j + 1], t[j]
			end
		end
	end
end

w = {5, 6, 4, 7, 9, 0, 8, 3, 1, 2}
sort_bubble(w)

for i = 1, #w do
	print(w[i])
end







