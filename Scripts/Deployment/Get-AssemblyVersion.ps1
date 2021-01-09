#
# Get the assembly version from an XML file.
#
# @author Michel Megens
# @email  michel@michelmegens.net
#

$content = Get-Content $args[0]
$xml = [Xml] $content
$assemblyVersion = [string] $xml.Project.PropertyGroup.AssemblyVersion
$length = $assemblyVersion.Length

if($length -Lt 8) {
    $version = $assemblyVersion;
} else {
    $version = $assemblyVersion.Substring(0, $length - 3)
}

echo $version
