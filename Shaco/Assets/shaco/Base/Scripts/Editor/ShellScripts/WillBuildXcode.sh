#!/bin/sh

rootPath=$(cd `dirname $0`;pwd)
xcodeProjPath=$1
shouldBuildPackage=$2

#创建日志文件，并打开
touch $xcodeProjPath"/xcode_build.log"
open $xcodeProjPath"/xcode_build.log"

#导出工程路径到其他shell脚本使用
export xcodeProjPath
export shouldBuildPackage

#开始编译xcode并输出日志
sh $rootPath"/BuildXcode.sh" | tee -a $xcodeProjPath"/xcode_build.log"