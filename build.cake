var target = Argument("target", "Default");

Task("Default")
	.Does(() => {
		Information("Building Sensate IoT");
	});

Task("Nuget-Update")
    .Does(() =>
{
    var files = GetFiles("./**/*.csproj");
    foreach(var file in files)
    {
        var content = System.IO.File.ReadAllText(file.FullPath);
        var matches = System.Text.RegularExpressions.Regex.Matches(content, @"PackageReference Include=""([^""]*)"" Version=""([^""]*)""");
        Information($"Updating {matches.Count} reference(s) from {file.GetFilename()}");
        foreach (System.Text.RegularExpressions.Match match in matches) {
            var packageName = match.Groups[1].Value;
            Information($"  Updating package {packageName}");
            var exitCode = StartProcess("cmd.exe",
                new ProcessSettings {
                    Arguments = new ProcessArgumentBuilder()
                        .Append("/C")
                        .Append("dotnet")
                        .Append("add")
                        .Append(file.FullPath)
                        .Append("package")
                        .Append(packageName)
                }
            );
        }
    }
});

RunTarget(target);
