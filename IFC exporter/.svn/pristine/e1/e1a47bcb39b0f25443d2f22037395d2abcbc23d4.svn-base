::$CODE_OWNER Orson_Liu
::$SECONDARY_OWNERS 
::$ACCESS_RESTRICTED No

REM This post build batch file is used to copy the output msi to the solution output folder for build farm, so this batch file can be removed when this project is put into open source.

echo on

set ThisBatFileRoot=%~dp0
set TargetMsi=%~1
set BuildConfiguration=%~2

set RevitAdditionsFolder=%ThisBatFileRoot%..\..\..\..\


for /f %%f in ('dir "%RevitAdditionsFolder%" /b /o-d') do (
	echo %%f
	REM Check whether the folder name contains the Build Configuration: Release Debug.
	call :CheckContainReleaseDebug %%f
)
goto :eof


REM Check whether the folder name contains the Build Configuration: Release Debug.
:CheckContainReleaseDebug %1
	set folderName=%1
	set string_temp_Release=%folderName:Release=%
	set string_temp_Debug=%folderName:Debug=%
	if /I "%BuildConfiguration%"=="Release" (
		if /I "%folderName%" NEQ "%string_temp_Release%" (call :CheckContainX64Win32 %folderName%)
	) else (
		if /I "%folderName%" NEQ "%string_temp_Debug%" (call :CheckContainX64Win32 %folderName%)
	)
goto :eof


REM Check whether the folder name contains x64 or Win32.
:CheckContainX64Win32 %1
	set folderName=%1
	REM Check whether the folder name contains x64.
	set string_temp=%folderName:x64=%
	if /I "%folderName%" NEQ "%string_temp%" (set SolutionOutputFolder=%folderName%&& goto:CopyToSolutionOutput)
	REM Check whether the folder name contains Win32.
	set string_temp=%folderName:Win32=%
	if /I "%folderName%" NEQ "%string_temp%" (set SolutionOutputFolder=%folderName%&& goto:CopyToSolutionOutput)
goto :eof


:CopyToSolutionOutput
	copy /y "%TargetMsi%" "%RevitAdditionsFolder%%SolutionOutputFolder%\"
goto :end

:end
exit
