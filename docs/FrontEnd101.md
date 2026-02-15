# FrontEnd 101 (ASP.NET MVC)


1) การเขียน Views เบื้องต้น
2) ความเชื่อมโยงระหว่าง Views และ Controller
3) DTO (Data Transfer Object) ใช้ส่งข้อมูลขึ้นหน้า
4) การสร้าง ViewComponent

---

## 1) Views คืออะไร

**View** คือไฟล์หน้าจอที่ผู้ใช้เห็น (HTML + Razor syntax) โดยอยู่ในโฟลเดอร์ `Views/` ตามชื่อ Controller

โครงสร้างมาตรฐาน:

```
Views/
	Home/
		Index.cshtml
	Auth/
		Login.cshtml
	Shared/
		_Layout.cshtml
```

### Razor Syntax เบื้องต้น

- เขียน C# ใน view ได้ด้วย `@`
- ตัวอย่าง:

```cshtml
@{
		ViewData["Title"] = "Home";
}

<h1>@ViewData["Title"]</h1>

@if (ViewBag.isAuth)
{
		<p>ยินดีต้อนรับ</p>
}
```

### การใช้ Layout

Layout คือโครงหน้าเว็บหลัก เช่น navbar/footer เพื่อ reuse ในทุกหน้า

`Views/Shared/_Layout.cshtml` เป็นไฟล์หลัก และในแต่ละหน้าให้ประกาศว่าใช้ layout นี้ผ่าน `_ViewStart.cshtml`

ตัวอย่าง `_ViewStart.cshtml`:

```cshtml
@{
		Layout = "_Layout";
}
```

### ใส่ CSS เฉพาะหน้า

ในหน้าแต่ละ view สามารถกำหนด section ของ CSS ได้ เช่น:

```cshtml
@section Styles {
	<link rel="stylesheet" href="~/css/home.css" asp-append-version="true" />
}
```

และใน `_Layout.cshtml` ต้องมี `@RenderSection("Styles", required: false)` เพื่อให้ section นี้ทำงาน

---

## 2) ความเชื่อมโยงระหว่าง View และ Controller

### หลักการทำงาน

1) ผู้ใช้เรียก URL
2) Controller รับ request แล้วเลือก View ที่จะ render
3) ส่งข้อมูลไปที่ View ผ่าน `ViewData`, `ViewBag`, หรือ `Model`

ตัวอย่าง Controller:

```csharp
public class HomeController : Controller
{
		public IActionResult Index()
		{
				ViewBag.isAuth = true;
				return View(); // จะหา Views/Home/Index.cshtml
		}
}
```

### การส่ง Model ไปที่ View

สร้าง DTO หรือ Model แล้วส่งไปยัง View:

```csharp
public IActionResult Profile()
{
		var model = new UserProfileDto { Name = "Alice" };
		return View(model);
}
```

ใน View:

```cshtml
@model UserProfileDto

<p>@Model.Name</p>
```

### การแมป URL กับ Controller

ค่า default อยู่ใน `Program.cs` (เช่น `MapControllerRoute`) เช่น:

```
/Home/Index => HomeController.Index()
/Auth/Login => AuthController.Login()
```

ถ้าใน Controller คืน `View("CustomName")` จะไปหา `Views/<Controller>/CustomName.cshtml`

---

## 3) DTO คืออะไร และใช้เมื่อไร

**DTO (Data Transfer Object)** คือคลาสสำหรับส่งข้อมูลไปยัง View โดยไม่ต้องส่ง Entity ตรงๆ
ข้อดีคือควบคุมข้อมูลที่ส่งออก ลดความเสี่ยง และอ่านง่าย

### ตัวอย่าง DTO ง่ายๆ

```
DTO/
  UserResponseDto.cs
```

```csharp
public class UserResponseDto
{
	public string Name { get; set; } = "";
	public string ProfileUrl { get; set; } = "";
}
```

### ใช้ DTO ใน Controller

```csharp
public IActionResult Profile()
{
	var dto = new UserResponseDto
	{
		Name = "Alice",
		ProfileUrl = "/images/non-profile-login.svg"
	};

	return View(dto);
}
```

### ใช้ DTO ใน View

```cshtml
@model UserResponseDto

<img src="@Model.ProfileUrl" class="login-icon" />
<span>@Model.Name</span>
```

### แนวคิดที่ควรจำ

- ไม่ส่ง Entity ตรงไป View ถ้าไม่จำเป็น
- DTO ควรเล็กและเฉพาะหน้าที่ใช้งาน
- ถ้า View ใช้ข้อมูลหลายส่วน ให้รวมเป็น ViewModel หรือ DTO เดียวที่เหมาะสม

---

## 4) ViewComponent คืออะไร

ViewComponent คือ “ชิ้นส่วน UI ที่ reuse ได้” เช่น Navbar, Footer, หรือ Card ต่างๆ

### โครงสร้างไฟล์

```
ViewComponents/
	NavbarViewComponent.cs
Views/
	Shared/
		Components/
			Navbar/
				Default.cshtml
```

### ตัวอย่าง ViewComponent

ไฟล์ C#:

```csharp
public class NavbarViewComponent : ViewComponent
{
		public IViewComponentResult Invoke()
		{
				return View();
		}
}
```

ไฟล์ View ของ component:

```cshtml
<nav class="nav-bar">
	<a href="/">Home</a>
</nav>
```

### การเรียกใช้ ViewComponent ใน View

```cshtml
@await Component.InvokeAsync("Navbar")
```

### ส่งค่าเข้า ViewComponent

ใน C#:

```csharp
public IViewComponentResult Invoke(string title)
{
		ViewBag.Title = title;
		return View();
}
```

ใน View:

```cshtml
@await Component.InvokeAsync("Navbar", new { title = "หน้าแรก" })
```

---

## สรุปสั้นๆ

- View คือหน้าจอหลัก อยู่ในโฟลเดอร์ `Views/`
- Controller คือคนเลือกว่าจะ render View ไหน และส่งข้อมูลให้ View
- DTO คือคลาสส่งข้อมูลไปยัง View โดยควบคุมสิ่งที่ต้องการแสดง
- ViewComponent คือชิ้นส่วน UI ที่ reusable ได้ เช่น Navbar

ถ้าต้องเริ่มหน้าใหม่ ให้คิดตามลำดับนี้:

1) สร้าง action ใน Controller
2) สร้าง View ให้ตรงกับ action
3) สร้าง DTO หากต้องส่งข้อมูลหลายค่าไปหน้า
4) ถ้ามีส่วนที่ reusable ให้แยกเป็น ViewComponent


