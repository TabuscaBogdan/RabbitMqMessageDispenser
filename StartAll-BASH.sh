#!/bin/bash

xterm -e dotnet 'Broker/bin/Debug/netcoreapp2.1/Broker.dll' 1  -sl 10000 &
xterm -e dotnet 'Broker/bin/Debug/netcoreapp2.1/Broker.dll' 2 -sl 10000 &
xterm -e dotnet 'Broker/bin/Debug/netcoreapp2.1/Broker.dll' 3 -sl 10000 &

sleep 3
xterm -e dotnet 'Consumer/bin/Debug/netcoreapp2.1/Consumer.dll' 1 1 -sl 10000 &
xterm -e dotnet 'Consumer/bin/Debug/netcoreapp2.1/Consumer.dll' 2 2 -sl 10000 &
xterm -e dotnet 'Consumer/bin/Debug/netcoreapp2.1/Consumer.dll' 3 3 -sl 10000 &

sleep 3
xterm -e dotnet 'Publisher/bin/Debug/netcoreapp2.1/Publisher.dll' 1 -sl 10000 &
xterm -e dotnet 'Publisher/bin/Debug/netcoreapp2.1/Publisher.dll' 2 -sl 10000 &
xterm -e dotnet 'Publisher/bin/Debug/netcoreapp2.1/Publisher.dll' 3 -sl 10000 &
