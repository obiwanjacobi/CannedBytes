CALL CreatePackagesDir.cmd

cd Code
CALL CleanAll.cmd
CALL BuildAll.cmd
CALL PackageCopyAll.cmd
cd ..

pause