# Set-ExecutionPolicy Unrestricted
Start-Process powershell.exe -ArgumentList @('dotnet', 'Broker\bin\Debug\netcoreapp2.1\Broker.dll', '@(1)')
Start-Process powershell.exe -ArgumentList @('dotnet', 'Broker\bin\Debug\netcoreapp2.1\Broker.dll', '@(2)')
Start-Process powershell.exe -ArgumentList @('dotnet', 'Broker\bin\Debug\netcoreapp2.1\Broker.dll', '@(3)')
Start-Sleep -Second 5
Start-Process powershell.exe -ArgumentList @('dotnet', 'Consumer\bin\Debug\netcoreapp2.1\Consumer.dll', '@(1)')
Start-Process powershell.exe -ArgumentList @('dotnet', 'Consumer\bin\Debug\netcoreapp2.1\Consumer.dll', '@(2)')
Start-Process powershell.exe -ArgumentList @('dotnet', 'Consumer\bin\Debug\netcoreapp2.1\Consumer.dll', '@(3)')
Start-Sleep -Second 25
Start-Process powershell.exe -ArgumentList @('dotnet', 'Publisher\bin\Debug\netcoreapp2.1\Publisher.dll', '@(1)')
Start-Process powershell.exe -ArgumentList @('dotnet', 'Publisher\bin\Debug\netcoreapp2.1\Publisher.dll', '@(2)')
Start-Process powershell.exe -ArgumentList @('dotnet', 'Publisher\bin\Debug\netcoreapp2.1\Publisher.dll', '@(3)')
