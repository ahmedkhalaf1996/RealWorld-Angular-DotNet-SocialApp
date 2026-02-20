@echo off 
SETLOCAL EnableExtensions

echo Staring All Microservices...

echo Starting Backend Api..
start "Backend API" cmd /k "cd /d "%~dp0backend\api" && dotnet run --urls=http://localhost:5000"


echo Starting RealtimeChat ..
start "RealtimeChat" cmd /k "cd /d "%~dp0backend\realTimeChat" && dotnet run --urls=http://localhost:8001"


echo Starting RealtimeNotification..
start "RealtimeNotification" cmd /k "cd /d "%~dp0backend\realtimeNotification" && dotnet run --urls=http://localhost:8088"

echo Press any key to exit this launcher ..
pause 