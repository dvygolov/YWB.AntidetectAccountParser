```                
                         Antidetect Accounts Parser  
    _            __     __  _ _             __          __  _     
   | |           \ \   / / | | |            \ \        / / | |    
   | |__  _   _   \ \_/ /__| | | _____      _\ \  /\  / /__| |__  
   | '_ \| | | |   \   / _ \ | |/ _ \ \ /\ / /\ \/  \/ / _ \ '_ \ 
   | |_) | |_| |    | |  __/ | | (_) \ V  V /  \  /\  /  __/ |_) |
   |_.__/ \__, |    |_|\___|_|_|\___/ \_/\_/    \/  \/ \___|_.__/ 
           __/ |                                                  
          |___/             https://yellowweb.top                 

If you like this script, PLEASE DONATE!  
WebMoney: Z182653170916  
Bitcoin: bc1qqv99jasckntqnk0pkjnrjtpwu0yurm0qd0gnqv  
Ethereum: 0xBC118D3FDE78eE393A154C29A4545c575506ad6B  
```

# Antidetect Accounts Parser by [Yellow Web](https://yellowweb.top)
Статья по софту на русском языке [находится тут](https://yellowweb.top/massovyj-import-akkauntov-facebook-v-profili-indigo-na-izi/)

This program automatically: 
- parses a file with Facebook accounts (or zip/rar-files with 1 FB account for each file)
- checks if accounts are valid (two types of checks are performed)
- creates a profile for each account in your Antidetect browser
- imports cookies into the profile
- saves all account's info into profile's note

Right now this software supports 4 Antidetect browsers: 
- [Indigo](https://yellowweb.top/indigo) - (btw, you can use **YELLOW** promocode to get 50% cashback)
- [Dolphin Anty](https://yellowweb.top/dolphinanty)
- [AdsPower](https://yellowweb.top/adspower)
- [OctoBrowser](https://octobrowser.net)

Also you can import your accounts to monitoring services. Right now these are supported:
- [FbTool](https://yellowweb.top/fbtool) 
- [Dolphin](https://yellowweb.top/dolphin)

**YELLOW** promocode will give you a discount for both!

# How to use this software
1. Compile the source code
2. Start your browser and leave it opened (except Octo, you don't need to launch it for the program to work)
3. Create **proxy.txt** file in the compiled program's directory
4. Add your proxies there line by line in this format - proxytype:ip:port:login:password, for example, *socks:133.43.23.4:10003:julia:sanders*
5. If you use [Dolphin Anty](https://yellowweb.top/dolphinanty) you can also add your proxy's update IP url, for example: *socks:133.43.23.4:10003:julia:sanders:yourproxy.com/erxc34sxre/update.php*
6. If you have multiple accounts in a text file then rename this file to **accounts.txt** and put it into the program's folder
7. If you have accounts in archives then create **"logs"** folder and put them there. Remember, there should be 1 account per 1 zip. You can also use Folders not archives, also 1 folder for one account.
8. Start the program and follow the instructions.

# Browsers
## Dolphin Anty
You can create **dolphinanty.txt** file and add your login and password there separated by :. 
## AdsPower
For Adspower you can create **adspower.txt** and add your login and password there separated by :. 
## OctoBrowser
For Octo you can create **octo.txt** and add your API token there.

# Monitoring services 
## FbTool 
You can create **fbtool.txt** and add your API token there, 
## Dolphin
You can create **dolphin.txt** and add your domain (WITHOUT http://) and API token separated by :. How to get API token? [Read here](https://documenter.getpostman.com/view/15402503/TzJrBJdk) or just run this script in your browser's console on your Dolphin's website:

*const cookie=document.cookie.split(";").map(function(o){return o.trim().split("=").map(decodeURIComponent)}).reduce(function(o,e){try{o[e[0]]=JSON.parse(e[1])}catch(c){o[e[0]]=e[1]}return o},{});console.log(cookie.user_id+"-"+cookie.hash);*


