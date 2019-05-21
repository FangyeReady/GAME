#include "Frame.h"
#include "Main.h"
#include "SceneManager.h"
#include <iostream>
#include <time.h>
#include "BeginScene.h"

//���г���������ʹ�õĹ��߱���
CGameInput* g_GameInput;
CGameOutput* g_GameOutput;

void Init()
{
	//��������
	g_GameInput = new CGameInput(g_hWnd);
	g_GameOutput = new CGameOutput(g_hWnd);

	//�������г���
	CSceneManager::GetSceneManager()->LoadScene(new CBeginScene);

	//���õ�ǰ����
	CSceneManager::GetSceneManager()->SetRunScene(0);

	//���г����ĳ�ʼ��
	for (int i = 0; i < CSceneManager::GetSceneManager()->m_Scenes.size(); ++i)
		CSceneManager::GetSceneManager()->m_Scenes[i]->Init(); //��̬
}

void Run()
{
	//��������
	g_GameInput->Run();

	//�����ʼ
	g_GameOutput->Begin();

	//��ǰ����������
	CSceneManager::GetSceneManager()->m_RunScene->Run(); //��̬

	//�������
	g_GameOutput->End();
}

void End()
{
	//���г����Ľ���
	for (int i = 0; i < CSceneManager::GetSceneManager()->m_Scenes.size(); ++i)
	{
		//���ó�������
		CSceneManager::GetSceneManager()->m_Scenes[i]->End(); //��̬

		//�ͷų���
		delete CSceneManager::GetSceneManager()->m_Scenes[i];
	}

	//�ͷŹ���
	CSceneManager::ReleaseSceneManager();
	delete g_GameOutput;
	delete g_GameInput;
}