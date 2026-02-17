# 🚀 Where We Go

Project Web application Developement [CE-KMITL] with .NET 10

## 🛠 Prerequisites


- .NET 10.0 SDK
- Docker Desktop (สำหรับรัน Database)
- Entity Framework Core Tools (ติดตั้งผ่านคำสั่ง: `dotnet tool install --global dotnet-ef`)

## ⚡ Quick Start

รัน 3 คำสั่งนี้เพื่อเริ่มใช้งานโปรเจกต์ทันที:

```bash
# 1. Start Database
docker-compose up -d

#ถ้ายังไม่มี migrations
dotnet ef migrations add

# 2. Update Database Schema
dotnet ef database update

# 3. Run Project (Watch Mode)
dotnet watch run
```

## Project Commands

### Database Management (EF Core)

- **Create Migration**: `dotnet ef migrations add <Name>`
- **Update Database**: `dotnet ef database update`
- **Remove Migration**: `dotnet ef migrations remove`

### Running Project

- **Build Project**: `dotnet build`
- **Run Project**: `dotnet run`
- **Hot Reload** (Recommended): `dotnet watch run`
- **Clean Project**: `dotnet clean`


## Setup Step-by-Step

1. **เตรียม environemnt**: ก็อปปี้ `.env.example` แล้วเปลี่ยนชื่อเป็น `.env` จากนั้นในไฟล์ จะมี DB_PASSWORD ให้เปลี่ยนเป็นของคุณ 

1. **เตรียม Database **: ใช้คำสั่ง `docker-compose up -d` เพื่อเปิดใช้งาน Database container (รอประมาณ 5-10 วินาทีเพื่อให้ระบบภายในพร้อมทำงาน)

2. **ตรวจสอบการเชื่อมต่อ**: หากต้องการเช็คสถานะ Database ให้ใช้คำสั่ง `docker-compose ps`

3. **เตรียม Schema**: รัน `dotnet ef database update` เพื่อสร้าง Table ต่างๆ ใน Database

4. **เริ่มพัฒนา**: ใช้ `dotnet watch run` เพื่อเริ่มรันโปรแกรม โดยระบบจะรีโหลดให้อัตโนมัติเมื่อมีการแก้ไข Code

## Member

<a href="https://github.com/paaw-potsawee">@paaw-potsawee</a>
<br>
<a href="https://github.com/Nanach1ll">@Nanach1ll </a>
<br>
<a href="https://github.com/Bokutosimp">@Bokutosimp </a>
<br>
<a href="https://github.com/ChevFy">@ChevFy </a>
<br>
<a href="https://github.com/Meridian6792">@Merdian6792</a>
