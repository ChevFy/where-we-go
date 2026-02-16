# FrontEnd 101 (ASP.NET MVC)


1) การเขียน Views เบื้องต้น
2) ความเชื่อมโยงระหว่าง Views และ Controller
3) DTO (Data Transfer Object) ใช้ส่งข้อมูลขึ้นหน้า
4) การสร้าง ViewComponent
5) การใช้งาน wwwroot สำหรับ CSS, JS และ Static Files

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

## 5) การใช้งาน wwwroot สำหรับ CSS, JS และ Static Files

### wwwroot คืออะไร

**wwwroot** คือโฟลเดอร์พิเศษที่เก็บไฟล์ static ทั้งหมด เช่น CSS, JavaScript, รูปภาพ, fonts เป็นต้น โฟลเดอร์นี้เป็นเพียงโฟลเดอร์เดียวที่ผู้ใช้สามารถเข้าถึงไฟล์ได้โดยตรงผ่าน URL

### โครงสร้างมาตรฐานของ wwwroot

```
wwwroot/
	css/
		site.css
		home.css
		loginpage.css
		components/
			navbar.css
			button.css
	js/
		site.js
		validation.js
	images/
		logo.png
		banner.jpg
	lib/
		bootstrap/
		jquery/
```

### การเขียน CSS ใน wwwroot

#### 1) สร้างไฟล์ CSS

สร้างไฟล์ใน `wwwroot/css/` เช่น `wwwroot/css/home.css`:

```css
.container {
	max-width: 1200px;
	margin: 0 auto;
	padding: 20px;
}

.card {
	background: white;
	border-radius: 8px;
	box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}
```

#### 2) เรียกใช้ CSS ใน View

มี 2 วิธีหลักๆ:

**วิธีที่ 1: ใส่ใน Layout สำหรับทุกหน้า**

ในไฟล์ `Views/Shared/_Layout.cshtml`:

```cshtml
<head>
	<link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
</head>
```

**วิธีที่ 2: ใส่เฉพาะหน้าผ่าน Section**

ในไฟล์ View เช่น `Views/Home/Index.cshtml`:

```cshtml
@section Styles {
	<link rel="stylesheet" href="~/css/home.css" asp-append-version="true" />
}
```

และใน `_Layout.cshtml` ต้องมี:

```cshtml
<head>
	<!-- CSS อื่นๆ -->
	@RenderSection("Styles", required: false)
</head>
```

### การเขียน JavaScript ใน wwwroot

#### 1) สร้างไฟล์ JS

สร้างไฟล์ใน `wwwroot/js/` เช่น `wwwroot/js/site.js`:

```javascript
// ตัวอย่าง JavaScript ง่ายๆ
function handleClick() {
	alert('Hello from JavaScript!');
}

// เมื่อโหลดหน้าเสร็จ
document.addEventListener('DOMContentLoaded', function() {
	console.log('Page loaded!');
});
```

#### 2) เรียกใช้ JavaScript ใน View

**วิธีที่ 1: ใส่ใน Layout สำหรับทุกหน้า**

ในไฟล์ `Views/Shared/_Layout.cshtml` (ใส่ก่อนปิด `</body>`):

```cshtml
<body>
	<!-- เนื้อหาหน้าเว็บ -->
	
	<script src="~/js/site.js" asp-append-version="true"></script>
</body>
```

**วิธีที่ 2: ใส่เฉพาะหน้าผ่าน Section**

ในไฟล์ View:

```cshtml
@section Scripts {
	<script src="~/js/validation.js" asp-append-version="true"></script>
	<script>
		// หรือเขียน inline script
		document.querySelector('.btn').addEventListener('click', function() {
			console.log('Button clicked!');
		});
	</script>
}
```

และใน `_Layout.cshtml` ต้องมี:

```cshtml
<body>
	<!-- เนื้อหาหน้าเว็บ -->
	
	@RenderSection("Scripts", required: false)
</body>
```

### การใช้รูปภาพและไฟล์ Static อื่นๆ

#### เก็บรูปภาพใน wwwroot/images/

```
wwwroot/
	images/
		logo.png
		non-profile-login.svg
		banner.jpg
```

#### เรียกใช้ในไฟล์ HTML/View

```cshtml
<img src="~/images/logo.png" alt="Logo" />
<img src="~/images/non-profile-login.svg" class="profile-icon" />
```

#### เรียกใช้ในไฟล์ CSS

```css
.hero {
	background-image: url('/images/banner.jpg');
	background-size: cover;
}
```

### สัญลักษณ์ ~ (Tilde) คืออะไร

`~` หมายถึง root ของโฟลเดอร์ `wwwroot` เมื่อใช้ใน tag helpers ของ ASP.NET

ตัวอย่าง:
- `~/css/site.css` → `/css/site.css` (ชี้ไปที่ `wwwroot/css/site.css`)
- `~/images/logo.png` → `/images/logo.png` (ชี้ไปที่ `wwwroot/images/logo.png`)

### asp-append-version คืออะไร

`asp-append-version="true"` เป็น tag helper ที่ใช้เพิ่มเวอร์ชันให้กับไฟล์เพื่อแก้ปัญหา browser cache

ตัวอย่าง:
```cshtml
<link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
```

จะกลายเป็น:
```html
<link rel="stylesheet" href="/css/site.css?v=abc123xyz" />
```

ข้อดี:
- เมื่อแก้ไขไฟล์ CSS/JS เบราว์เซอร์จะโหลดไฟล์ใหม่ทันที
- ไม่ต้อง hard refresh (Ctrl+F5) ทุกครั้ง

### ตัวอย่างการจัดการไฟล์สำหรับหน้า Home

#### 1) สร้าง CSS
`wwwroot/css/home.css`:
```css
.hero-section {
	background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
	padding: 60px 20px;
	color: white;
	text-align: center;
}

.card-grid {
	display: grid;
	grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
	gap: 20px;
	padding: 40px 20px;
}
```

#### 2) สร้าง JavaScript
`wwwroot/js/home.js`:
```javascript
document.addEventListener('DOMContentLoaded', function() {
	const cards = document.querySelectorAll('.card');
	
	cards.forEach(card => {
		card.addEventListener('click', function() {
			this.classList.toggle('active');
		});
	});
});
```

#### 3) ใช้ใน View
`Views/Home/Index.cshtml`:
```cshtml
@{
	ViewData["Title"] = "หน้าแรก";
}

@section Styles {
	<link rel="stylesheet" href="~/css/home.css" asp-append-version="true" />
}

<div class="hero-section">
	<h1>ยินดีต้อนรับ</h1>
	<p>ค้นหาสถานที่ท่องเที่ยวที่ใช่สำหรับคุณ</p>
</div>

<div class="card-grid">
	<div class="card">Card 1</div>
	<div class="card">Card 2</div>
	<div class="card">Card 3</div>
</div>

@section Scripts {
	<script src="~/js/home.js" asp-append-version="true"></script>
}
```

### แนวทางปฏิบัติที่ดี (Best Practices)

1. **แยก CSS/JS ตาม Component หรือหน้า** - ง่ายต่อการจัดการและ maintain
   - `home.css`, `loginpage.css`, `navbar.css`

2. **ใช้ asp-append-version เสมอ** - แก้ปัญหา cache
   ```cshtml
   <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
   ```

3. **ใส่ CSS ใน `<head>` และ JS ก่อนปิด `</body>`** - ช่วยให้หน้าเว็บโหลดเร็วขึ้น

4. **จัดโครงสร้างไฟล์ให้เป็นระเบียบ**
   ```
   wwwroot/
   	css/
   		components/  <- CSS ของ ViewComponent
   		pages/       <- CSS ของแต่ละหน้า
   		site.css     <- CSS global
   ```

5. **ใช้ Library จาก CDN หรือ NPM** - สำหรับ library ยอดนิยมเช่น Bootstrap, jQuery
   - เก็บไว้ใน `wwwroot/lib/`

6. **ตั้งชื่อไฟล์ให้มีความหมาย** - เช่น `loginpage.css`, `validation.js`

---

## สรุปสั้นๆ

- View คือหน้าจอหลัก อยู่ในโฟลเดอร์ `Views/`
- Controller คือคนเลือกว่าจะ render View ไหน และส่งข้อมูลให้ View
- DTO คือคลาสส่งข้อมูลไปยัง View โดยควบคุมสิ่งที่ต้องการแสดง
- ViewComponent คือชิ้นส่วน UI ที่ reusable ได้ เช่น Navbar
- wwwroot คือที่เก็บไฟล์ static ทั้งหมด (CSS, JS, รูปภาพ) และใช้ `~` เพื่ออ้างอิงไฟล์

ถ้าต้องเริ่มหน้าใหม่ ให้คิดตามลำดับนี้:

1) สร้าง action ใน Controller
2) สร้าง View ให้ตรงกับ action
3) สร้าง DTO หากต้องส่งข้อมูลหลายค่าไปหน้า
4) ถ้ามีส่วนที่ reusable ให้แยกเป็น ViewComponent
5) สร้างไฟล์ CSS/JS ใน `wwwroot` และอ้างอิงใน View ด้วย `asp-append-version="true"`


