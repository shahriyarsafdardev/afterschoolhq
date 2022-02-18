
setlocal enabledelayedexpansion
set path="C:\temp\app"

if "%DEPLOYMENT_GROUP_NAME%"=="" (
    echo "DEPLOYMENT_GROUP_NAME does not exist"
) else (
    echo "DEPLOYMENT_GROUP_NAME=%DEPLOYMENT_GROUP_NAME%"
	
	if "%DEPLOYMENT_GROUP_NAME%"=="AfterSchool-QA-Group" (
	  set target="C:\websites\afterschoolhq\afterschoolhq-qa\afterschool"
	)

	if "%DEPLOYMENT_GROUP_NAME%"=="AfterSchool-CI-Group" (
	  set target="C:\websites\afterschoolhq\afterschoolhq-ci\afterschool"
	)

	if "%DEPLOYMENT_GROUP_NAME%"=="AfterSchool-Staging-Group" (
	  set target="C:\websites\afterschoolhq\afterschoolhq-staging\afterschool"
	)

	if "%DEPLOYMENT_GROUP_NAME%"=="AfterSchool-Production-Group" (
	  set target="C:\websites\afterschoolhq\afterschoolhq-production\afterschool"
	)

	if "%DEPLOYMENT_GROUP_NAME%"=="AfterSchool-RiseProject-Group" (
	  set target="C:\websites\afterschoolhq\afterschoolhq-riseproject\afterschool"
	)

	
	if exist !target! rmdir /s /q !target!

	mkdir !target!

	%systemroot%\System32\robocopy %path% !target! /e /xf "Microsoft.Data.SqlClient.SNI.x64.dll"

    echo %ERRORLEVEL%

	if %ERRORLEVEL% LSS 8 goto finish

    Echo Something failed & goto :eof

    :finish
    Echo All done, no fatal errors.
)
