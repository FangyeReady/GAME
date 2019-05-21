#!/bin/sh

rootPath=$(cd `dirname $0`;pwd)
shaco_path_flag="GitHub/shaco"

if [[ $rootPath =~ $shaco_path_flag ]]; then
	projectPath=${rootPath%shaco/Base/*}shaco/Base
else
	projectPath=${rootPath%Assets/*}
fi

#进入工程目录
cd $projectPath

echo "rootPath="$rootPath>$rootPath"/log.tmp"
echo "projectPath="$projectPath>>$rootPath"/log.tmp"
echo "param1="$1" param2="$s2>>$rootPath"/log.tmp"

#回滚配置
if [[ "$2" = "DiscardAllLocalChanges" ]]; then

	echo "project will discard all changes">>$rootPath"/log.tmp"

	if [[ $1 = "Svn" ]]; then
		svn revert . -R
	else
		git stash save --include-untracked
		git stash drop
	fi

	echo "project discard all changes end">>$rootPath"/log.tmp"

else

	#更新工程
	echo "project will update">>$rootPath"/log.tmp"
	
	if [[ $1 = "Svn" ]]; then
		svn update
	else
		git pull
	fi

	#检查工程是否有修改内容
	if [[ $1 = "Svn" ]]; then
		diff_tmp=$(svn status)
	else
		diff_tmp=$(git diff)
	fi
	
	echo $diff_tmp>$rootPath"/diff.tmp"

	echo "project update end">>$rootPath"/log.tmp"

fi

ls -l && exit