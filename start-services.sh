#!/bin/bash

echo "Starting Azurite"
sudo docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 -v /home/craig/.local/azurite mcr.microsoft.com/azure-storage/azurite &

echo "Starting Open Project" 
sudo docker run -i -p 5500:80 -e SECRET_KEY_BASE=secret -e OPENPROJECT_HOST__NAME=localhost:8080 -e OPENPROJECT_HTTPS=false -e OPENPROJECT_DEFAULT__LANGUAGE=en openproject/openproject:dev &
