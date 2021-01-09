
try {
    $repo = $args[0]
    $tag = $args[1]
    $url = "https://hub.docker.com/v2/repositories/$repo/tags/$tag"
    Invoke-RestMethod -Uri $url > $null

    echo "exists"
} catch {
    echo "failed"
}
