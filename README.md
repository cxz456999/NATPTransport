How to use NATPTransport
-------------
#### Support protocols
- TCP
- UDP (Coming soon)
- SSL (Coming soon)
- WebSocket (Coming soon)

## Server
#### Step1 Setup
##### Install .NET Core
`$sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-  prod.rpm`

`$sudo yum install dotnet-sdk-3.1`

##### Build 
`$cd /home/user/NATP_Server`

`$dotnet build`

##### Config 
`$nano /home/user/NATP_Server/NATP_Server/App.config`
- External_IP_Address:  Server external IP address
- Port: Server listening on
- Users:  List of users have authority to create room on server

[![Appconfig](https://github.com/cxz456999/NATPTransport/blob/master/Images/Appconfig.JPG "Appconfig")](https://github.com/cxz456999/NATPTransport/blob/master/Images/Appconfig.JPG "App.config")

#### Step3 Run
`$/home/user/NATP_Server/NATP_Server/bin/Debug/netcoreapp3.1`

`$sudo ./NATP_Server`

## Client
1. Import [Mirror](https://github.com/vis2k/Mirror "Mirror")
2. Import NATPTransport.unitypackage
3. Use NATPTransport as transport
4. Input the public IP address (server external-ip), User/Password (added in App.config)
[![example](https://github.com/cxz456999/NATPTransport/blob/master/Images/Example.JPG "example")](https://github.com/cxz456999/NATPTransport/blob/master/Images/Example.JPG "example")
5. Input room tag
6. enjoyed
