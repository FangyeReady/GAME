#ifndef _SCENE_H_
#define _SCENE_H_

class CScene
{
public:

	//��������Ϊ�麯�����Ա�֤��delete
	//����ָ���ʱ�����������Ҳ������
	virtual ~CScene();

	//������ʼ��
	virtual void Init();

	//��������
	virtual void Run();

	//��������
	virtual void End();
};

#endif