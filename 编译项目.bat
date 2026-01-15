@echo off
cd /d "%~dp0"

echo ========================================
echo     Mir2 项目编译脚本
echo ========================================
echo.

REM 检查 MSBuild 是否存在
where msbuild >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [错误] 未找到 MSBuild！
    echo.
    echo 请尝试以下方法之一：
    echo.
    echo 1. 安装 Visual Studio (推荐)
    echo    下载: https://visualstudio.microsoft.com/downloads/
    echo.
    echo 2. 安装 .NET Framework 4.5 开发工具
    echo    下载: https://dotnet.microsoft.com/download/dotnet-framework/net45
    echo.
    echo 3. 安装 Build Tools for Visual Studio
    echo    下载: https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2019
    echo.
    pause
    exit /b 1
)

echo [1/2] 正在编译服务端...
msbuild "Server\Server.csproj" /p:Configuration=Release /p:Platform="Any CPU" /v:minimal
if %ERRORLEVEL% neq 0 (
    echo [错误] 服务端编译失败！
    pause
    exit /b 1
)
echo [成功] 服务端编译完成
echo.

echo [2/2] 正在编译客户端...
msbuild "Client\Client.csproj" /p:Configuration=Release /p:Platform="Any CPU" /v:minimal
if %ERRORLEVEL% neq 0 (
    echo [错误] 客户端编译失败！
    pause
    exit /b 1
)
echo [成功] 客户端编译完成
echo.

echo ========================================
echo     编译完成！
echo ========================================
echo.
echo 服务端输出: Server\bin\Release\Server.exe
echo 客户端输出: Client\bin\Release\Client.exe
echo.
echo 按任意键启动服务端和客户端...
pause >nul

start "" "Server\bin\Release\Server.exe"
timeout /t 2 /nobreak >nul
start "" "Client\bin\Release\Client.exe"

echo.
echo 服务端和客户端已启动！
