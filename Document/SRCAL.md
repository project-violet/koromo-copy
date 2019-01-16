# SRCAL Script


## 단부루 예제

```
##
## Koromo Copy SRCAL Script
##
## Danbooru Downloader
##

##
## Attributes
##
$ScriptName = "danbooru-pages"
$ScriptVersion = "0.1"
$ScriptAuthor = "dc-koromo"
$ScriptFolderName = "danbooru"
$ScriptRequestName = "danbooru"
$URLSpecifier = "https://danbooru.donmai.us/"
$UsingDriver = 0

##
## Procedure
##
request_url = $RequestURL
max_page = $Infinity

loop (i = 1 to max_page) [
    $LoadPage(url(request_url, "&page=", i))
    sub_urls = cal("/html[1]/body[1]/div[1]/div[3]/div[1]/section[1]/div[3]/div[1]/article[{1+i*1}]/a[1], #attr[href], #front[https://danbooru.donmai.us")

    foreach (sub_url : sub_urls) [
        $LoadPage(sub_url)
        if equal(cal("/html[1]/body[1]/div[1]/div[3]/div[1]/section[1]/div[2]/span[1]/a[1], #attr[id]"), "image-resize-link") [
            image_url = cal("/html[1]/body[1]/div[1]/div[3]/div[1]/section[1]/div[2]/span[1]/a[1], #attr[href]")
        ] else [
            image_url = cal("/html[1]/body[1]/div[1]/div[3]/div[1]/section[1]/section[1]/img[1], #attr[src]")
        ]
        file_name = split(image_url, '/')[-1]
        $AppendImage(image_url, file_name)
    ]

    if equal($LatestImagesCount, 0) [ 
        exit loop
    ]
]

$RequestDownload
```