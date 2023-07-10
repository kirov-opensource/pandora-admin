# pandora-admin

## 已办

- [x] 1、支持多账号
- [x] 2、支持多WebToken
- [x] 3、移除JS文件，通过代码进行替换。

## 待办

- [ ] 1、管理页面
- [ ] 2、支持并发询问
- [ ] 3、将配置文件转为env
- [ ] 4、支持ARM
- [ ] 5、头像正常展示
- [ ] 6、自动初始化DB

## 部署
* 1、 首先需要一个MySQL DB，推荐免费的MySQL服务[planetscale](https://app.planetscale.com/)，然后执行[初始化SQL文件](Initial.sql)。
* 2、 找台Linux服务器，安装好`Docker`和`Docker compose`。
* 3、 找一个目录开始部署。
* 4、 编辑pandora.admin.json文件
  
```json
{
  "ConnectionStrings": {
    "Default": "Server=<NEED TO REPLACE>;Database=pandora_admin;user=root;password=password;",
  },
  "JWTSecurityKey": "<NEED TO REPLACE>",
  "ReverseProxy": {
    "HttpRequest": {
      "AllowResponseBuffering": false
    },
    "Routes": {
      "route1": {
        "ClusterId": "pandora_server",
        "Match": {
          "Path": "{**catch-all}"
        }
      },
      "route2": {
        "ClusterId": "gpt_server",
        "Match": {
          "Path": "/gpt/{**catch-all}"
        },
        "Transforms": [
          {
            "PathPattern": "{**catch-all}"
          }
        ]
      }
    },
    "Clusters": {
      "pandora_server": {
        "Destinations": {
          "pandora_server/destination1": {
            "Address": "http://pandora:80"
          }
        }
      },
      "gpt_server": {
        "Destinations": {
          "gpt_server/destination1": {
            "Address": "https://ai-20230703.fakeopen.com"
          }
        }
      }
    }
  }
}
```

配置项解释

|配置项|解释|
|---|-----|
|ConnectionStrings.Default|DB连接字符串|
|JWTSecurityKey|JWT的key，随便打一串字符就行，比如10*SDAy98zxcya89|

* 5、 编辑`docker-compose.yaml`文件。

```yaml
version: "3.9"
services:
  pandora:
    image: pengzhile/pandora
    container_name: pandora
    restart: always
    volumes:
      - ./data:/data
    environment:
      - PANDORA_SERVER=0.0.0.0:80
      - PANDORA_CLOUD=1
      - PANDORA_VERBOSE=1
      - CHATGPT_API_PREFIX=http://IP:5001/gpt

  pandora-admin:
    image: ghcr.io/kirov-opensource/pandora-admin:release
    container_name: pandora_admin
    restart: always
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    ports:
      - "5001:80"
    volumes:
      - ./pandora.admin.json:/app/appsettings.Production.json
```

配置项解释

|配置项|解释|
|---|-----|
|CHATGPT_API_PREFIX|外网入口，比如走Nginx或者Caddy反代了，那就配置反代服务的入口域名，带上/gpt，比如域名是`https://chat.example.com`，那就改成`https://chat.example.com/gpt`，这个配置和pandora-admin.ports\[0\]之间没有必然联系，可以直接IP+PORT访问pandora-admin的服务，也可以使用反代包裹访问。|
|pandora-admin.ports\[0\]|暴露出去的端口，如果不想暴露在外网上，可以修改成`127.0.0.1:5001:80`|

* 6、 启动
```bash
docker compose up -d
```
