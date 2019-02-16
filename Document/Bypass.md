# 보안접속을 위한 통신 암호화 방법

아래 서술된 방법들을 사용하면 보다 안전하게 인터넷에 접속할 수 있습니다.

MTU 변경방식은 권장하지 않습니다.

## 1. ESNI

```
FireFox
about:config
network.security.esni.enabled = true
network.trr.mode = 2
network.trr.bootstrapAddress = 1.1.1.1
```

```
Chromium
```

## 2. DPI Bypass

```
GoodbyeDPI
https://github.com/ValdikSS/GoodbyeDPI
```

## 3. DNS over HTTPS

```
Linux
Cloudflared
https://developers.cloudflare.com/1.1.1.1/dns-over-https/cloudflared-proxy/
```

```
Windows
Secret DNS
http://secretdns.kilho.net/
```

```
Android
intra
https://play.google.com/store/apps/details?id=app.intra&hl=ko
```

```
IOS
```