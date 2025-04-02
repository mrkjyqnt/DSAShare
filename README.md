# DSAShare
## Introduction
DSAShare (Datamex Saint Adeline Share) is a local file-sharing system designed to securely share files without requiring technical knowledge of networking to locate files. **Note**: This system was created primarily for educational purposes. Security vulnerabilities have not been thoroughly evaluated - use at your own risk.

## Technology Stack
- **Frontend**: Windows Presentation Foundation (WPF)  
- **Backend**: VB.NET 9.0  
- **Database**: Microsoft SQL Server  
- **Architecture**: MVVM with Prism  

## 📦 Core Packages (Critical Dependencies)
| Package | Version | Purpose |
|---------|---------|---------|
| `Prism.Core` | 9.0.537 | MVVM framework core |
| `Prism.DryIoc` | 9.0.537 | Dependency injection |
| `Prism.Wpf` | 9.0.537 | WPF integration |
| `Microsoft.Data.SqlClient` | 6.0.1 | Secure database access |
| `Microsoft.Xaml.Behaviors` | 1.1.135 | UI interactivity |
| `FreeSpire.Office` | 8.2.0 | Office file processing |

## 🚀 Setup Guide
1. **Prerequisites**:
    - Visual Studio 2022 (with .NET Desktop workload)
    - SQL Server 2019+ (Client and Server)

2. **First-time setup**:
   ```powershell
   git clone https://github.com/mrkjyqnt/DSAShare.git
   cd DSAShare
   dotnet restore

3. **Configuration**: Create file named config.ini:
    ```powershell
    [Database]
    DB_SERVER=(Server IP)\SQLEXPRESS
    DB_NAME=dsa_share_database
    DB_USER=(Server Database Username)
    DB_PASSWORD=(Server Database Password)
    
    [Network]
    FOLDER_PATH=\\(Server IP)\ServerStorage\
    NET_USER=(Server Windows Username)
    NET_PASSWORD=(Server Windows Password)
    ```
    - Copy the file to the Debug Folder under your project path
    
4. **Setup the server and client**: 
    - Install SQL Server Configuration Manager
    - Open the Application
    - Open `SQL Server Network Configuration > Protocols For SQLExpress > TCP/IP`
    - Under IP Addresses tab, scroll down, set the "`IP All`" TCP Port to `1433`, then click ok
    - Go to Windows firewall, and create New Rules for inbound and outbound
        - Rule Type: `Port`
        - Protocol and Ports: `TCP` | Specific local ports: `1433`
        - Action: `Allow Connection`
        - Profile: `Check All`
        - Name: `SQL Server Sharing TCP`
    - Do the same for Client Computer
    
5. **Important**
    - Make sure to create a file under `C:\` name it `ServerStorage` and Right-Click the folder then click `Properties`, click `Share` and select the User *(The User here should be the same as the User from the `Configuration > [Network] > NET_USER and NET_PASSWORD` re-read number 3.)* you want to add, and enable `Read/Write`
    - Make the server IP Static
    - On Networks, make sure to choose Discovery to Private

