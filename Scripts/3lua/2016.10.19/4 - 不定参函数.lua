function f1(...)
	--����ǲ����κ����������ͨ��
	--����Ĵ���õ����еĲ�������
	--Щ��������һ�����и�ֵ��a
	local a = {...}
	for k, d in pairs(a) do
		print(k .. "-" .. tostring(d))
	end
end

f1(100, 200, "abc", 300, false)
--��������У��ᴫ���{100,200,"abc",300,false}

function get_add(...)
	local a = {...}
	local b = 0
	for i = 1, #a do
		b = b + a[i]
	end
	return b
end

print(get_add(1, 2, 3, 4))
print(get_add(1, 2, 3, 4, 5, 6, 7))
