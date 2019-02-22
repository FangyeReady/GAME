#ifndef _SCENE_H_
#define _SCENE_H_

class CScene
{
public:

	//析构设置为虚函数可以保证在delete
	//父类指针的时候子类的析构也被调用
	virtual ~CScene();

	//场景初始化
	virtual void Init();

	//场景运行
	virtual void Run();

	//场景结束
	virtual void End();
};

#endif