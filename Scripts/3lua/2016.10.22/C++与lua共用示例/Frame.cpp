#include "Frame.h"
#include "Main.h"
#include "SceneManager.h"
#include <iostream>
#include <time.h>
#include "BeginScene.h"

//所有场景都可以使用的工具变量
CGameInput* g_GameInput;
CGameOutput* g_GameOutput;

void Init()
{
	//创建工具
	g_GameInput = new CGameInput(g_hWnd);
	g_GameOutput = new CGameOutput(g_hWnd);

	//加载所有场景
	CSceneManager::GetSceneManager()->LoadScene(new CBeginScene);

	//设置当前场景
	CSceneManager::GetSceneManager()->SetRunScene(0);

	//所有场景的初始化
	for (int i = 0; i < CSceneManager::GetSceneManager()->m_Scenes.size(); ++i)
		CSceneManager::GetSceneManager()->m_Scenes[i]->Init(); //多态
}

void Run()
{
	//输入运行
	g_GameInput->Run();

	//输出开始
	g_GameOutput->Begin();

	//当前场景的运行
	CSceneManager::GetSceneManager()->m_RunScene->Run(); //多态

	//输出结束
	g_GameOutput->End();
}

void End()
{
	//所有场景的结束
	for (int i = 0; i < CSceneManager::GetSceneManager()->m_Scenes.size(); ++i)
	{
		//调用场景结束
		CSceneManager::GetSceneManager()->m_Scenes[i]->End(); //多态

		//释放场景
		delete CSceneManager::GetSceneManager()->m_Scenes[i];
	}

	//释放工具
	CSceneManager::ReleaseSceneManager();
	delete g_GameOutput;
	delete g_GameInput;
}