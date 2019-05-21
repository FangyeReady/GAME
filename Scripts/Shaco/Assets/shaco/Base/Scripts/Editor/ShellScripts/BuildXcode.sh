#!/bin/sh

rootPath=$(cd `dirname $0`;pwd)
projectPath=${rootPath%Assets/*}

cd $xcodeProjPath

#编译xcode并自动退出
xcodebuild || exit

echo "<<<<<<<<<<<<<<<<<<<<<<Build Xcode End>>>>>>>>>>>>>>>>>>>>" >> $xcodeProjPath"/xcode_build.log"

#仅编译的话，就不用执行下面的打包流程了
if [[ "$shouldBuildPackage" != "buildPackage=true" ]]; then
    exit 1
fi

#编译文件用的文件夹路径
archive_path_tmp="${xcodeProjPath}/build.xcarchive"

#工程对象名字
target_name_tmp="Unity-iPhone"

#xcode工程配置文件路径
project_name_tmp="${xcodeProjPath}/${target_name_tmp}.xcodeproj"

#打包plist配置文件路径
export_option_plist_tmp=$rootPath"/iOS/export_option.plist"

#生成编译文件
xcodebuild archive -project ${project_name_tmp} \
                   -scheme ${target_name_tmp} \
                   -archivePath ${archive_path_tmp}
       
package_path_tmp=$xcodeProjPath"/build/$target_name_tmp.ipa"       

#导出ipa到本地包目录
xcodebuild -exportArchive -archivePath $archive_path_tmp \
           -exportPath $package_path_tmp \
           -exportOptionsPlist $export_option_plist_tmp

echo "<<<<<<<<<<<<<<<<<<<<<<Build ipa End>>>>>>>>>>>>>>>>>>>>" >> $xcodeProjPath"/xcode_build.log"
           
#打开导出的ipa包目录
open $package_path_tmp

#重新格式化安装包名字
package_path_tmp=$package_path_tmp"/"$target_name_tmp".ipa"