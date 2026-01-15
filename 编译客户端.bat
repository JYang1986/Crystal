@echo off
setlocal

echo ========================================
echo     Mir2 客户端编译脚本
echo ========================================
echo.

set MSBUILD="D:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
set PROJECT="Client\Client.csproj"
set CONFIG=Debug

echo MSBuild 路径: %MSBUILD%
echo 项目文件: %PROJECT%
echo 配置: %CONFIG%
echo.

if not exist %MSBUILD% (
    echo [错误] 找不到 MSBuild
    pause
    exit /b 1
)

if not exist %PROJECT% (
    echo [错误] 找不到项目文件
    pause
    exit /b 1
)

echo [1/2] 正在编译客户端...
echo.

%MSBUILD% %PROJECT% /p:Configuration=%CONFIG% /p:Platform="Any CPU" /v:minimal /nologo

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [错误] 编译失败！错误代码: %ERRORLEVEL%
    pause
    exit /b 1
)

echo.
echo [成功] 编译完成！
echo.
echo 输出目录: Client\bin\%CONFIG%\
echo.
echo 按任意键运行客户端...
pause >nul

if exist "Client\bin\%CONFIG%\Client.exe" (
    start "" "Client\bin\%CONFIG%\Client.exe"
    echo.
    echo 客户端已启动！
) else (
    echo [错误] 找不到编译后的 Client.exe
)

echo.
pause
