

set H=R:\KSP_1.2.2_dev
echo %H%

copy WernherChecker\bin\Debug\WernherChecker.dll GameData\WernherChecker\Plugins
xcopy /E /Y GameData\WernherChecker %H%\GameData\WernherChecker
