@echo off
REM 设置控制台编码为 UTF-8
chcp 65001 >nul 2>nul

cd /d "%~dp0"

cls
echo ========================================
echo     Mir2 Project Launcher
echo     Mir2 项目启动器
echo ========================================
echo.

REM 检查 .NET SDK
where dotnet >nul 2>nul
if %ERRORLEVEL% equ 0 (
    echo [OK] .NET SDK detected
    goto :check_msbuild
)

REM 检查 MSBuild
where msbuild >nul 2>nul
if %ERRORLEVEL% equ 0 (
    echo [OK] MSBuild detected
    goto :menu
)

echo [WARNING] .NET build tools not found!
echo.
echo You need to install one of the following:
echo.
echo 1. Visual Studio Community (Recommended, Free)
echo    Download: https://visualstudio.microsoft.com/downloads/
echo    Install ".NET desktop development" workload
echo.
echo 2. Build Tools for Visual Studio (Smaller, compile only)
echo    Download: https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022
echo    Install ".NET desktop build tools"
echo.

:check_msbuild
where msbuild >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [INFO] VS Code found, but MSBuild is required to compile
    echo.
    echo Please install Visual Studio or Build Tools
    start https://visualstudio.microsoft.com/downloads/
    pause
    exit /b 1
)

:menu
echo.
echo Please select an option / 请选择操作:
echo.
echo 1. Build Server and Client / 编译服务端和客户端
echo 2. Build Server only / 只编译服务端
echo 3. Build Client only / 只编译客户端
echo 4. Start Server / 启动服务端
echo 5. Start Client / 启动客户端
echo 6. Open in VS Code / 用VS Code打开
echo 7. View Build Guide / 查看编译指南
echo.
set /p choice=Enter choice (1-7) / 输入选择:

if "%choice%"=="1" goto :build_all
if "%choice%"=="2" goto :build_server
if "%choice%"=="3" goto :build_client
if "%choice%"=="4" goto :run_server
if "%choice%"=="5" goto :run_client
if "%choice%"=="6" goto :open_vscode
if "%choice%"=="7" goto :show_guide

echo Invalid choice! / 无效选择！
pause
goto :menu

:build_all
echo.
echo [1/2] Building Server / 编译服务端...
msbuild "Server\Server.csproj" /p:Configuration=Release /p:Platform="Any CPU" /v:minimal
if %ERRORLEVEL% neq 0 (
    echo [ERROR] Server build failed! / 服务端编译失败！
    pause
    goto :menu
)
echo [OK] Server built successfully / 服务端编译完成

echo.
echo [2/2] Building Client / 编译客户端...
msbuild "Client\Client.csproj" /p:Configuration=Release /p:Platform="Any CPU" /v:minimal
if %ERRORLEVEL% neq 0 (
    echo [ERROR] Client build failed! / 客户端编译失败！
    pause
    goto :menu
)
echo [OK] Client built successfully / 客户端编译完成

echo.
echo Build complete! / 编译完成！
echo.
echo Start game? / 启动游戏？ (Y/N)
set /p start_game=Enter choice:
if /i "%start_game%"=="Y" (
    start "" "Server\bin\Release\Server.exe"
    timeout /t 2 /nobreak >nul
    start "" "Client\bin\Release\Client.exe"
)
goto :menu

:build_server
echo.
echo Building Server / 编译服务端...
msbuild "Server\Server.csproj" /p:Configuration=Release /p:Platform="Any CPU" /v:minimal
if %ERRORLEVEL% neq 0 (
    echo [ERROR] Build failed! / 编译失败！
) else (
    echo [OK] Build complete / 编译完成
)
pause
goto :menu

:build_client
echo.
echo Building Client / 编译客户端...
msbuild "Client\Client.csproj" /p:Configuration=Release /p:Platform="Any CPU" /v:minimal
if %ERRORLEVEL% neq 0 (
    echo [ERROR] Build failed! / 编译失败！
) else (
    echo [OK] Build complete / 编译完成
)
pause
goto :menu

:run_server
echo.
echo Starting Server / 启动服务端...
if exist "Server\bin\Release\Server.exe" (
    start "" "Server\bin\Release\Server.exe"
    echo [OK] Server started / 服务端已启动
) else (
    echo [ERROR] Server executable not found! / 未找到编译后的服务端文件！
    echo Please build project first / 请先编译项目
)
pause
goto :menu

:run_client
echo.
echo Starting Client / 启动客户端...
if exist "Client\bin\Release\Client.exe" (
    start "" "Client\bin\Release\Client.exe"
    echo [OK] Client started / 客户端已启动
) else (
    echo [ERROR] Client executable not found! / 未找到编译后的客户端文件！
    echo Please build project first / 请先编译项目
)
pause
goto :menu

:open_vscode
echo.
echo Opening in VS Code / 用 VS Code 打开...
code .
goto :menu

:show_guide
echo.
echo Opening build guide / 打开编译指南...
if exist "design\编译运行指南.md" (
    start "" "design\编译运行指南.md"
) else (
    echo [ERROR] Guide not found! / 文档未找到！
)
goto :menu
