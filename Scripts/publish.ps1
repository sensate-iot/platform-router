#
# Publish the SensateService backend
#
# @author Michel Megens
# @email  dev@bietje.net
#

param(
	[switch]$clean = $false
)

if($clean) {
	Write-Host "Removing files..."
	Remove-Item -ErrorAction SilentlyContinue -path ApiCore\dist -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path Auth\dist -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path Core\dist -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path DatabaseTool\dist -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path MqttHandler\dist -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path Setup\dist -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path Tests\dist -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path WebSocketHandler\dist -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path dist\ -recurse -force
	
	Remove-Item -ErrorAction SilentlyContinue -path ApiCore\bin\Release -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path Auth\bin\Release -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path Core\bin\Release -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path DatabaseTool\bin\Release -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path MqttHandler\bin\Release -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path Setup\bin\Release -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path Tests\bin\Release -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path WebSocketHandler\bin\Release -recurse -force
	
	Remove-Item -ErrorAction SilentlyContinue -path ApiCore\obj\Release -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path Auth\obj\Release -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path Core\obj\Release -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path DatabaseTool\obj\Release -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path MqttHandler\obj\Release -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path Setup\obj\Release -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path Tests\obj\Release -recurse -force
	Remove-Item -ErrorAction SilentlyContinue -path WebSocketHandler\obj\Release -recurse -force
	exit
}

Write-Host "Publishing SensateService projects..."
Write-Host 'Pick any of the following targets:'
Write-Host '    win-x64'
Write-Host '    win-x86'
Write-Host '    linux-x64'
Write-Host '    linux-musl-x64'
Write-Host '    linux-arm'
Write-Host '    osx-x64'
$target = Read-Host -Prompt 'Input the target architecture'

# Build release mode
dotnet publish -c Release -r $target -o dist\MqttHandler MqttHandler\MqttHandler.csproj
dotnet publish -c Release -r $target -o dist\WebSocketHandler WebSocketHandler\WebSocketHandler.csproj
dotnet publish -c Release -r $target -o dist\Auth Auth\Auth.csproj
dotnet publish -c Release -r $target -o dist\Setup Setup\Setup.csproj
dotnet publish -c Release -r $target -o dist\DatabaseTool DatabaseTool\DatabaseTool.csproj

# Copy files to central location
