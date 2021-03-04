#!/bin/bash

cp server.cfg ../serverfiles/server/rustserver/cfg/server.cfg  
cp users.cfg ../serverfiles/server/rustserver/cfg/users.cfg
cp lgsm-config-lgsm-rustserver.cfg ../lgsm/config-lgsm/rustserver/rustserver.cfg
mkdir ../serverfiles/oxide
rm -rf ../serverfiles/oxide/plugins
rm -rf ../serverfiles/oxide/config
cp -r plugins ../serverfiles/oxide/plugins/
cp -r config ../serverfiles/oxide/config/
