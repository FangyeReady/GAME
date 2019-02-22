#ifndef _SCENE_MANAGER_H_
#define _SCENE_MANAGER_H_

#include "Scene.h"
#include <vector>

//场景管理器类（单件）
class CSceneManager
{
	//静态指针指向唯一的一份本类对象
	static CSceneManager* m_Obj;

	//将构造、拷贝构造设置为私有
	CSceneManager();
	CSceneManager(const CSceneManager& that);

public:

	//场景表
	std::vector<CScene*> m_Scenes;

	//正在运行的场景
	CScene* m_RunScene;

	//静态函数得到场景管理器
	static CSceneManager* GetSceneManager();

	//释放唯一的场景管理器
	static void ReleaseSceneManager();

	//我们在程序中用下面三个函数来完成场景的调度和程序的退出

	//加载场景
	int LoadScene(CScene* scene);

	//设置运行场景
	bool SetRunScene(int index);

	//得到指定场景
	CScene* GetScene(int index);

	//退出程序
	void ExitGame();
};

#endif