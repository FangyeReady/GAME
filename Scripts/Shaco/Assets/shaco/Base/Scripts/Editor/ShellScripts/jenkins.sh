#!/bin/sh

HTTP_PORT=$1
if [[ $1 -le 0 ]]; then
	HTTP_PORT=8080
fi

echo "HTTP_PORT="$HTTP_PORT

output_tmp=$(echo 123456 | sudo -S lsof -i:$HTTP_PORT)

echo "output="$output_tmp

pid=`echo $output_tmp | awk '{print $11}'`

if [[ $pid -gt 0 ]]; then
	echo "kill pid="$pid
	echo 123456 | sudo -S kill $pid
fi

echo $HTTP_PORT>"jenkins_port.tmp"

rootPath=$(cd `dirname $0`;pwd)

#打开ip映射到外网服务
open -a Terminal.app $rootPath"/ngrok.sh"

#打开jenkins服务
java -jar $rootPath"/jenkins.war" --httpPort=$HTTP_PORT