#!/bin/sh
HTTP_PORT=$(cat "jenkins_port.tmp")
rm -rf "jenkins_port.tmp"

echo "HTTP_PORT="$HTTP_PORT

rootPath=$(cd `dirname $0`;pwd)
$rootPath"/ngrok" http $HTTP_PORT
