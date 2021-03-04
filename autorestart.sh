#!/bin/bash
OUTPUT=$(/home/rustserver/rustserver monitor)
OUTPUT="$(echo $OUTPUT | sed $'s/\e\\[[0-9;:]*[a-zA-Z]//g')"
echo "${OUTPUT}"

okstring="[ OK ] Monitoring rustserver"
if [[ $OUTPUT == *"${okstring}"* ]]; then
  echo "Server is running"
else
   echo "Server need to be start! starting...."
  /home/rustserver/rustserver start
fi
