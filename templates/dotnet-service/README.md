# 📦 Layered ASP.NET Core .NET 10 Service Template

> Mẫu khung dự án backend chuẩn dành cho các Microservices thuộc dự án **FPT Alumni Connect** (PRN232).

---

## 🏛️ Cấu trúc Kiến trúc (Layered Architecture)

Template áp dụng mô hình 3 tầng phân lớp rõ ràng:

```text
Template.Api (REST API, Controllers, Middleware, Health, OpenAPI)
    │
    ▼ (Reference duy nhất)
Template.Services (Business Logic, Validation, DTOs, Exceptions)
    │
    ▼ (Reference duy nhất)
Template.Repositories (Data Access, EF Core SqlServer, Migrations, Entities)
```

> ⚠️ **Quy tắc bất di bất dịch**: `Template.Api` không được reference hay inject bất kỳ Repository nào trực tiếp. Mọi truy vấn và nghiệp vụ phải đi qua tầng `Template.Services`.

---

## 🗄️ Cơ sở Dữ liệu & EF Core Migrations

- **Provider**: Microsoft SQL Server (`Microsoft.EntityFrameworkCore.SqlServer`).
- **Tên Database mặc định**: `FAlumni_Template_Db` (trên instance `localhost\SQLEXPRESS` hoặc Docker SQL Server).
- **Cấu hình chuỗi kết nối**: Trong `src/Template.Api/appsettings.json`:
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=FAlumni_Template_Db;Trusted_Connection=True;TrustServerCertificate=True;"
  }
  ```

### Các lệnh EF Core thông dụng:

```bash
# 1. Liệt kê các migration hiện có
dotnet ef migrations list --project src/Template.Repositories --startup-project src/Template.Api

# 2. Tạo migration mới khi có thay đổi Entity/Fluent API
dotnet ef migrations add <MigrationName> --project src/Template.Repositories --startup-project src/Template.Api

# 3. Áp dụng migration vào SQL Server
dotnet ef database update --project src/Template.Repositories --startup-project src/Template.Api

# 4. Sinh file script SQL lũy đẳng để nộp bài hoặc triển khai
dotnet ef migrations script --idempotent --output migrations.sql --project src/Template.Repositories --startup-project src/Template.Api
```

---

## 🚀 Hướng dẫn Sử dụng Nhanh (Quickstart)

### 1. Khôi phục packages & Biên dịch dự án
```bash
# Khôi phục dependencies
dotnet restore

# Biên dịch toàn bộ Solution
dotnet build
```

### 2. Chạy Kiểm thử (xUnit Unit & Integration Tests)
```bash
dotnet test
```

### 3. Khởi chạy Web API cục bộ
```bash
dotnet run --project src/Template.Api/Template.Api.csproj
```
Mặc định ứng dụng sẽ lắng nghe trên cổng HTTP: `http://localhost:5000`.

---

## 📡 Danh sách Endpoints Mẫu (Sample Endpoints)

| Phương thức | Đường dẫn URL | Mô tả | Mã trạng thái HTTP |
| :--- | :--- | :--- | :--- |
| `GET` | `/health` | Kiểm tra tình trạng hoạt động (Health Check) | `200 OK` (`"Healthy"`) |
| `GET` | `/api/v1/sample-items` | Lấy danh sách toàn bộ các items | `200 OK` (JSON List) |
| `GET` | `/api/v1/sample-items/paged` | Lấy danh sách có phân trang (`pageNumber`, `pageSize`) và tìm kiếm | `200 OK` (`PagedResponse<T>`) |
| `GET` | `/api/v1/sample-items/{id}` | Lấy chi tiết 1 item theo GUID | `200 OK` hoặc `404 Not Found` |
| `POST` | `/api/v1/sample-items` | Tạo mới 1 sample item | `201 Created` (Kèm Location header) |
| `DELETE` | `/api/v1/sample-items/{id}` | Xóa 1 sample item theo GUID | `204 NoContent` hoặc `404 Not Found` |

---

## 🛠️ Hướng dẫn Sao chép cho Service Mới

Khi bắt đầu một service mới trong danh sách (`f-alumni-user-service`, `f-alumni-chat-service`, v.v.):

1. Copy toàn bộ nội dung thư mục này vào thư mục repository của service.
2. Đổi tên solution `Template.sln` thành `FAlumni.<ServiceName>.sln`.
3. Đổi tên các thư mục `src/Template.*` thành `src/FAlumni.<ServiceName>.*`.
4. Tìm và thay thế chuỗi `Template` thành `FAlumni.<ServiceName>` trong tất cả các file `.csproj` và `.cs`.
5. Cập nhật tên Database trong `appsettings.json` thành `FAlumni_<ServiceName>_Db` (xem [docs/05-database.md](file:///c:/Use/University/PRN232/ASM/guideline/docs/05-database.md)).
6. Chạy `dotnet test` để đảm bảo hệ thống biên dịch và chạy kiểm thử thành công trước khi bắt đầu viết code nghiệp vụ.
