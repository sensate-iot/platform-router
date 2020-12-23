
$content = Get-Content $args[0]

$xml = [Xml] $content
$version = $xml.Project.PropertyGroup.AssemblyVersion
echo $version