@echo off
REM 尝试使用 Visual Studio 打开解决方案

echo 正在查找 Visual Studio...

REM 尝试常见的 Visual Studio 路径
set "VS2022=C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe"
set "VS2019=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe"
set "VS2017=C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe"
set "VS2015=C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe"

if exist "%VS2022%" (
    echo 找到 Visual Studio 2022
    start "" "%VS2022%" "%~dp0Legend of Mir.sln"
    goto :end
)

if exist "%VS2019%" (
    echo 找到 Visual Studio 2019
    start "" "%VS2019%" "%~dp0Legend of Mir.sln"
    goto :end
)

if exist "%VS2017%" (
    echo 找到 Visual Studio 2017
    start "" "%VS2017%" "%~dp0Legend of Mir.sln"
    goto :end
)

if exist "%VS2015%" (
    echo 找到 Visual Studio 2015
    start "" "%VS2015%" "%~dp0Legend of Mir.sln"
    goto :end
)

echo 未找到 Visual Studio！
echo.
echo 请选择：
echo 1. 下载 Visual Studio Community (免费)
echo 2. 使用命令行编译
echo.
set /p choice=请输入选择 (1/2):

if "%choice%"=="1" (
    start https://visualstudio.microsoft.com/downloads/
    echo 请安装 Visual Studio，选择 ".NET 桌面开发" 工作负载
)

if "%choice%"=="2" (
    echo.
    echo 使用命令行编译项目：
    echo.
    echo 编译服务端：
    echo msbuild "Server\Server.csproj" /p:Configuration=Release
    echo.
    echo 编译客户端：
    echo msbuild "Client\Client.csproj" /p:Configuration=Release
    echo.
    pause
)

:end
pause
