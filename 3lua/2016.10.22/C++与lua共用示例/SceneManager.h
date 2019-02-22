#ifndef _SCENE_MANAGER_H_
#define _SCENE_MANAGER_H_

#include "Scene.h"
#include <vector>

//�����������ࣨ������
class CSceneManager
{
	//��ָ̬��ָ��Ψһ��һ�ݱ������
	static CSceneManager* m_Obj;

	//�����졢������������Ϊ˽��
	CSceneManager();
	CSceneManager(const CSceneManager& that);

public:

	//������
	std::vector<CScene*> m_Scenes;

	//�������еĳ���
	CScene* m_RunScene;

	//��̬�����õ�����������
	static CSceneManager* GetSceneManager();

	//�ͷ�Ψһ�ĳ���������
	static void ReleaseSceneManager();

	//�����ڳ�����������������������ɳ����ĵ��Ⱥͳ�����˳�

	//���س���
	int LoadScene(CScene* scene);

	//�������г���
	bool SetRunScene(int index);

	//�õ�ָ������
	CScene* GetScene(int index);

	//�˳�����
	void ExitGame();
};

#endif