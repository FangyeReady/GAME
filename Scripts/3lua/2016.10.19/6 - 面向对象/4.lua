a = {}

a.y = 1
a.z = 2

function a:f(x)
	print(self.y + self.z + x)
end

--�����ڶ��庯��f��ʱ�򣬲����õ�.����
--�õ�:����:��������ĺ���Ĭ����һ������
--����self���ĸ�����ı��ڵ���f��������ô
--self���Զ�����ֵΪ�ĸ���ģ�������this

a:f(3)

--�����f����Ҳ����дΪ���µ����

a.ff = function (self, x)
	print(self.y + self.z + x)
end

a:ff(3)
