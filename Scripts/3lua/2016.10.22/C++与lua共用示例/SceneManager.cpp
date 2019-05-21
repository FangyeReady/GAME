#include "SceneManager.h"
#include <windows.h>

CSceneManager* CSceneManager::m_Obj = NULL;

CSceneManager::CSceneManager()
:
m_RunScene(0)
{}

CSceneManager::CSceneManager(const CSceneManager& that){}

CSceneManager* CSceneManager::GetSceneManager()
{
	if (NULL == m_Obj)
		m_Obj = new CSceneManager;
	return m_Obj;
}

void CSceneManager::ReleaseSceneManager()
{	
	if (NULL != m_Obj)
	{
		delete m_Obj;
		m_Obj = NULL;
	}
}

int CSceneManager::LoadScene(CScene* scene)
{
	m_Scenes.push_back(scene);
	return (int)m_Scenes.size() - 1;
}

bool CSceneManager::SetRunScene(int index)
{
	if (index < 0 || index >= (int)m_Scenes.size())
		return false;

	//设置运行场景
	m_RunScene = m_Scenes[index];

	return true;
}

CScene* CSceneManager::GetScene(int index)
{
	if (index < 0 || index >= (int)m_Scenes.size())
		return 0;
	else
		return m_Scenes[index];
}

void CSceneManager::ExitGame()
{
	PostQuitMessage(0);
}