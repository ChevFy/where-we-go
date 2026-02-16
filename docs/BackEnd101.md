# Backend 101: ASP.NET Core MVC

## MVC Pattern คืออะไร?

MVC (Model-View-Controller) เป็นรูปแบบการออกแบบ (Design Pattern) ที่แยกส่วนต่างๆ ของแอปพลิเคชันออกเป็น 3 ส่วนหลัก:

```
┌──────────────────────────────────────┐
│         User (ผู้ใช้งาน)             │
└──────────────┬───────────────────────┘
               │
               ▼
       ┌───────────────┐
       │     VIEW      │  ← ส่วนที่แสดงผล (UI)
       └───────┬───────┘
               │
               ▼
       ┌───────────────┐
       │  CONTROLLER   │  ← ตัวควบคุมการทำงาน
       └───────┬───────┘
               │
               ▼
       ┌───────────────┐
       │     MODEL     │  ← ข้อมูลและ Business Logic
       └───────────────┘
```

### ความหมายแต่ละส่วน:

- **Model**: เก็บข้อมูล และโครงสร้างของข้อมูล (Entity, DTO)
- **View**: ส่วนที่แสดงผลให้ผู้ใช้เห็น (HTML, CSS, JavaScript)
- **Controller**: ตัวกลางที่รับ Request จากผู้ใช้และตัดสินใจว่าจะทำอะไร

---

## Controller คืออะไร?

**Controller** คือตัวควบคุมการทำงานของแอปพลิเคชัน ทำหน้าที่เป็นตัวกลางรับ HTTP Request จากผู้ใช้ แล้วตัดสินใจว่าจะ:

1. เรียกใช้ Service ไหน
2. ประมวลผลข้อมูลอย่างไร
3. ส่ง Response กลับไปในรูปแบบไหน (View, JSON, Redirect)

### โครงสร้างของ Controller

```csharp
namespace where_we_go.Controllers;

public class HomeController : Controller
{
    // Action Methods อยู่ที่นี่
}
```

### ส่วนประกอบสำคัญ:

1. **ต้อง inherit จาก `Controller` class**
2. **ชื่อต้องลงท้ายด้วย `Controller`** (เช่น `HomeController`, `UserController`)
3. **มี Action Methods** ที่รับ HTTP Request

---

## Action Methods คืออะไร?

**Action Method** คือ Method ภายใน Controller ที่รับ HTTP Request และส่ง Response กลับ

### ตัวอย่างจาก HomeController

```csharp
public class HomeController(UserManager<User> userManager) : Controller
{
    private UserManager<User> _userManager { get; init; } = userManager;
    
    // Action Method สำหรับหน้าแรก
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        bool IsAuth = User.Identity?.IsAuthenticated ?? false;
        ViewBag.IsAuth = IsAuth;
        return View();  // ส่ง View กลับไป
    }

    // Action Method ที่ต้อง Login ก่อน
    [Authorize]
    public async Task<IActionResult> Privacy()
    {
        bool IsAuth = User.Identity?.IsAuthenticated ?? false;
        return View();
    }
}
```

### HTTP Verbs ที่ใช้บ่อย:

- `[HttpGet]` - ดึงข้อมูล (แสดงหน้าเว็บ, ดึง API)
- `[HttpPost]` - ส่งข้อมูล (สมัครสมาชิก, Login, สร้างโพสต์)
- `[HttpPut]` - อัปเดตข้อมูลทั้งหมด
- `[HttpPatch]` - อัปเดตข้อมูลบางส่วน
- `[HttpDelete]` - ลบข้อมูล

---

## ตัวอย่างการทำงานของ Controller

### ตัวอย่างที่ 1: Login (AuthController)

```csharp
public class AuthController(SignInManager<User> signInManager, UserManager<User> userManager) : Controller
{
    private SignInManager<User> _signInManager { get; init; } = signInManager;
    private UserManager<User> _userManager { get; init; } = userManager;

    // แสดงหน้า Login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // รับข้อมูลจากฟอร์ม Login
    [HttpPost]
    public async Task<IActionResult> Login(LoginDto model)
    {
        // 1. ตรวจสอบข้อมูลที่ส่งมา
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // 2. หา User จาก Email
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "No account found with this email");
            return View(model);
        }

        // 3. ตรวจสอบรหัสผ่าน
        var result = await _signInManager.PasswordSignInAsync(
            user, 
            model.Password, 
            isPersistent: true, 
            lockoutOnFailure: false
        );

        // 4. ถ้าสำเร็จ redirect ไปหน้าแรก
        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Home");
        }

        // 5. ถ้าไม่สำเร็จ แสดง Error
        ModelState.AddModelError(string.Empty, "Incorrect password. Please try again.");
        return View(model);
    }
}
```

### ตัวอย่างที่ 2: User Profile (UserController)

```csharp
public class UserController(UserManager<User> userManager) : Controller
{
    private UserManager<User> _userManager { get; init; } = userManager;

    // หน้าโปรไฟล์ (ต้อง Login ก่อน)
    [Authorize]
    public async Task<IActionResult> Me()
    {
        // 1. ดึง User ID จาก Claims
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Auth");
        
        // 2. ค้นหา User จาก Database
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }
        
        // 3. แปลงเป็น DTO และส่งไป View
        var userResponse = new UserResponseDto(user);
        return View(userResponse);
    }
}
```

---

## Service คืออะไร? ใช้ทำอะไร?

**Service** คือ class ที่เก็บ **Business Logic** (ตรรกะทางธุรกิจ) ของแอปพลิเคชัน

### ทำไมต้องมี Service?

X **แบบไม่ดี**: เขียน Logic ทั้งหมดใน Controller

```csharp
public async Task<IActionResult> CreatePost(PostDto model)
{
    // Validation
    if (string.IsNullOrEmpty(model.Title)) return BadRequest();
    
    // ตรวจสอบสิทธิ์
    var user = await _userManager.FindByIdAsync(userId);
    if (user.Role != UserRole.Admin) return Forbid();
    
    // สร้าง Post
    var post = new Post { 
        Title = model.Title,
        Content = model.Content
    };
    
    _dbContext.Posts.Add(post);
    await _dbContext.SaveChangesAsync();
    
    // ส่ง Notification
    await SendNotificationToFollowers(user.Id);
    
    return Ok();
}
```

**ปัญหา**:
- Controller มี Logic เยอะเกินไป
- ถ้ามี Controller อื่นต้องใช้ Logic เดียวกัน ต้อง Copy Code
- ทดสอบยาก

---

**แบบที่ดี**: แยก Business Logic ไปอยู่ใน Service

```csharp
// Controller - เรียบง่าย ชัดเจน
public class PostController(IPostService postService) : Controller
{
    private IPostService _postService { get; init; } = postService;

    [HttpPost]
    public async Task<IActionResult> CreatePost(PostDto model)
    {
        var result = await _postService.CreatePostAsync(model);
        
        if (!result.Success)
            return BadRequest(result.Error);
            
        return Ok(result.Data);
    }
}

// Service - เก็บ Logic ทั้งหมด
public class PostService(AppDbContext dbContext, INotificationService notificationService) : IPostService
{
    private AppDbContext _dbContext { get; init; } = dbContext;
    private INotificationService _notificationService { get; init; } = notificationService;

    public async Task<ServiceResult<Post>> CreatePostAsync(PostDto model)
    {
        // Validation
        if (string.IsNullOrEmpty(model.Title))
            return ServiceResult<Post>.Fail("Title is required");
        
        // สร้าง Post
        var post = new Post { 
            Title = model.Title,
            Content = model.Content,
            CreatedAt = DateTime.UtcNow
        };
        
        _dbContext.Posts.Add(post);
        await _dbContext.SaveChangesAsync();
        
        // ส่ง Notification
        await _notificationService.NotifyFollowersAsync(post.UserId);
        
        return ServiceResult<Post>.Success(post);
    }
}
```

### ประโยชน์ของ Service Layer:

**แยก Concerns** - Controller ทำหน้าที่รับ-ส่ง Request, Service ทำ Logic  
**Reusable** - Service สามารถเรียกใช้ซ้ำได้จากหลาย Controller  
**Testable** - ทดสอบ Logic ใน Service ได้ง่ายกว่า  
**Maintainable** - Code ดูแลง่าย แก้ไขที่เดียวใช้ได้ทุกที่

---

## โครงสร้าง Service ในโปรเจคนี้

```csharp
namespace where_we_go.Service
{
    // Interface - กำหนด Contract
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(string userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
    }

    // Implementation - ทำงานจริง
    public class UserService(AppDbContext appDbContext) : IUserService
    {
        private AppDbContext _dbContext { get; init; } = appDbContext;

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _dbContext.Users.FindAsync(userId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }
    }
}
```

---

## Primary Constructor คืออะไร?

**Primary Constructor** เป็นฟีเจอร์ใหม่ใน **C# 12** ที่ทำให้การเขียน Constructor สั้นลงมาก

### แบบเก่า (Traditional Constructor)

```csharp
public class UserController : Controller
{
    private readonly UserManager<User> _userManager;

    // ต้องเขียน Constructor เอง
    public UserController(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Me()
    {
        var user = await _userManager.FindByIdAsync(userId);
        // ...
    }
}
```

### แบบใหม่ (Primary Constructor) ⭐

```csharp
public class UserController(UserManager<User> userManager) : Controller
{
    private UserManager<User> _userManager { get; init; } = userManager;

    public async Task<IActionResult> Me()
    {
        var user = await _userManager.FindByIdAsync(userId);
        // ...
    }
}
```

### เปรียบเทียบ:

| แบบเก่า | แบบใหม่ (Primary Constructor) |
|---------|-------------------------------|
| 5+ บรรทัด | 1 บรรทัด |
| ต้องประกาศตัวแปร 2 ครั้ง | ประกาศครั้งเดียวในวงเล็บ |
| ต้องเขียน Constructor Body | ไม่ต้องเขียน Constructor |

---

## ตัวอย่าง Primary Constructor ในโปรเจคนี้

### ตัวอย่างที่ 1: Controller กับ 1 Dependency

```csharp
// HomeController รับ UserManager เข้ามา
public class HomeController(UserManager<User> userManager) : Controller
{
    private UserManager<User> _userManager { get; init; } = userManager;
    
    public async Task<IActionResult> Index()
    {
        // ใช้ _userManager ได้เลย
        var user = await _userManager.GetUserAsync(User);
        return View();
    }
}
```

### ตัวอย่างที่ 2: Controller กับหลาย Dependencies

```csharp
// AuthController รับ 2 Services เข้ามา
public class AuthController(
    SignInManager<User> signInManager, 
    UserManager<User> userManager
) : Controller
{
    private SignInManager<User> _signInManager { get; init; } = signInManager;
    private UserManager<User> _userManager { get; init; } = userManager;

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto model)
    {
        // ใช้ทั้ง 2 Services ได้เลย
        var user = await _userManager.FindByEmailAsync(model.Email);
        var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, false);
        
        if (result.Succeeded)
            return RedirectToAction("Index", "Home");
            
        return View(model);
    }
}
```

### ตัวอย่างที่ 3: Service กับ Primary Constructor

```csharp
// UserService รับ DbContext เข้ามา
public class UserService(AppDbContext appDbContext) : IUserService
{
    private AppDbContext _dbContext { get; init; } = appDbContext;

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        return await _dbContext.Users.FindAsync(userId);
    }
}
```

---

## การทำงานร่วมกันของ Controller และ Service

### Flow การทำงานแบบเต็ม:

```
User Request
    │
    ▼
┌─────────────────┐
│   Controller    │  1. รับ HTTP Request
│   (AuthController)│  2. Validate Input
└────────┬────────┘  3. เรียก Service
         │
         ▼
┌─────────────────┐
│    Service      │  4. ทำ Business Logic
│  (AuthService)  │  5. เข้าถึง Database
└────────┬────────┘  6. ประมวลผลข้อมูล
         │
         ▼
┌─────────────────┐
│    Database     │  7. CRUD Operations
│  (AppDbContext) │
└────────┬────────┘
         │
         ▼
    Service คืนผลลัพธ์
         │
         ▼
    Controller ตัดสินใจ
         │
         ▼
    ส่ง Response กลับ (View/JSON/Redirect)
```

### ตัวอย่าง Code จริง:

```csharp
// 1. Controller - รับ Request และเรียก Service
public class PostController(IPostService postService) : Controller
{
    private IPostService _postService { get; init; } = postService;

    [HttpPost]
    public async Task<IActionResult> Create(PostDto model)
    {
        // Validate
        if (!ModelState.IsValid)
            return View(model);

        // เรียก Service ทำงาน
        var post = await _postService.CreatePostAsync(model);

        // ส่ง Response
        return RedirectToAction("Details", new { id = post.Id });
    }
}

// 2. Service - Business Logic
public class PostService(AppDbContext dbContext, INotificationService notificationService) : IPostService
{
    private AppDbContext _dbContext { get; init; } = dbContext;
    private INotificationService _notificationService { get; init; } = notificationService;

    public async Task<Post> CreatePostAsync(PostDto model)
    {
        // สร้าง Entity
        var post = new Post
        {
            Title = model.Title,
            Content = model.Content,
            CreatedAt = DateTime.UtcNow
        };

        // บันทึกลง Database
        _dbContext.Posts.Add(post);
        await _dbContext.SaveChangesAsync();

        // Logic เพิ่มเติม - ส่ง Notification
        await _notificationService.NotifyNewPostAsync(post);

        return post;
    }
}
```

---

## Best Practices

### Controller

**ควรทำ**:
- เก็บ Logic น้อยที่สุด (Thin Controller)
- Validate Input ด้วย `ModelState`
- ใช้ `[Authorize]` ป้องกัน Endpoint
- Return `IActionResult` สำหรับความยืดหยุ่น

**ไม่ควรทำ**:
- เขียน Business Logic ใน Controller
- เข้าถึง Database โดยตรง
- มี Method ที่ยาวเกินไป (>50 บรรทัด)

### Service

**ควรทำ**:
- สร้าง Interface สำหรับทุก Service
- แยก Logic ออกเป็น Method เล็กๆ
- Handle Error อย่างชัดเจน
- ใช้ Async/Await สำหรับ I/O Operations

**ไม่ควรทำ**:
- Return HTML/View จาก Service (ให้ Controller จัดการ)
- Depend on HttpContext
- มี Static Method ที่มี Dependency

### Primary Constructor

**ควรทำ**:
- ใช้ `{ get; init; }` สำหรับ Property
- ตั้งชื่อ Parameter เป็น camelCase
- ตั้งชื่อ Property เป็น _camelCase หรือ PascalCase

**ไม่ควรทำ**:
- ใช้ Parameter จาก Primary Constructor โดยตรง (ควรเก็บใน Property)
- Mix แบบเก่ากับแบบใหม่ในไฟล์เดียวกัน

---

## 🚀 เริ่มต้นทำโปรเจค

1. **สร้าง Model** - กำหนดโครงสร้างข้อมูล
2. **สร้าง Service Interface & Implementation** - เขียน Business Logic
3. **Register Service** ใน `Program.cs` - `builder.Services.AddScoped<IUserService, UserService>()`
4. **สร้าง Controller** - รับ Request และเรียก Service
5. **สร้าง View** - แสดงผล UI

