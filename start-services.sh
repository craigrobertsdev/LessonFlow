#!/bin/bash

echo "Starting Azurite"
sudo docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 -v /home/craig/.local/azurite mcr.microsoft.com/azure-storage/azurite &