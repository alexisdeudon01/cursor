@echo off
REM ═══════════════════════════════════════════════════════════════
REM  Session Isolation Tests - Windows Batch Runner
REM  Usage: run_tests.bat [quick|normal|stress] [sessions] [players]
REM ═══════════════════════════════════════════════════════════════

setlocal

REM Configuration
set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe"
set PROJECT_PATH=%~dp0
set LOG_FILE=%PROJECT_PATH%test_results.log

REM Parse arguments
set TEST_TYPE=%1
if "%TEST_TYPE%"=="" set TEST_TYPE=normal

set SESSIONS=%2
if "%SESSIONS%"=="" set SESSIONS=5

set PLAYERS=%3
if "%PLAYERS%"=="" set PLAYERS=4

echo ═══════════════════════════════════════════════════════════════
echo  Session Isolation Tests
echo  Type: %TEST_TYPE%
echo  Sessions: %SESSIONS%
echo  Players per session: %PLAYERS%
echo ═══════════════════════════════════════════════════════════════
echo.

REM Check Unity exists
if not exist %UNITY_PATH% (
    echo ERROR: Unity not found at %UNITY_PATH%
    echo Please update UNITY_PATH in this script.
    exit /b 1
)

REM Select test method based on type
if "%TEST_TYPE%"=="quick" (
    set METHOD=SessionTests.CommandLineTests.RunQuick
) else if "%TEST_TYPE%"=="stress" (
    set METHOD=SessionTests.CommandLineTests.RunStress
) else if "%TEST_TYPE%"=="custom" (
    set METHOD=SessionTests.CommandLineTests.RunCustom
) else (
    set METHOD=SessionTests.CommandLineTests.Run
)

echo Running: %METHOD%
echo Log file: %LOG_FILE%
echo.

REM Run Unity in batch mode
if "%TEST_TYPE%"=="custom" (
    %UNITY_PATH% -batchmode -nographics -projectPath "%PROJECT_PATH%" -executeMethod %METHOD% -sessions %SESSIONS% -players %PLAYERS% -logFile "%LOG_FILE%" -quit
) else (
    %UNITY_PATH% -batchmode -nographics -projectPath "%PROJECT_PATH%" -executeMethod %METHOD% -logFile "%LOG_FILE%" -quit
)

set EXIT_CODE=%ERRORLEVEL%

echo.
echo ═══════════════════════════════════════════════════════════════
if %EXIT_CODE%==0 (
    echo  TESTS PASSED!
) else (
    echo  TESTS FAILED!
)
echo  Exit code: %EXIT_CODE%
echo  See %LOG_FILE% for details
echo ═══════════════════════════════════════════════════════════════

REM Show test report if it exists
if exist "%PROJECT_PATH%test_report.txt" (
    echo.
    echo Test Report:
    type "%PROJECT_PATH%test_report.txt"
)

exit /b %EXIT_CODE%
