# Quy chuẩn Đóng gói Container & Môi trường Docker Compose (Docker & Containerization Standards)

> **Mã tài liệu**: `DOC-12-DOCKER`  
> **Dự án**: FPT Alumni Connect (PRN232 Final Assignment)  
> **Trạng thái**: Hoàn thành & Phê duyệt  
> **Tệp Compose Mẫu**: [`docker/compose.demo.yml`](file:///c:/Use/University/PRN232/ASM/guideline/docker/compose.demo.yml) | **Tệp Môi trường Mẫu**: [`docker/.env.example`](file:///c:/Use/University/PRN232/ASM/guideline/docker/.env.example)

---

## 🎯 1. Mục tiêu & Ranh giới Sở hữu Hạ tầng

Tài liệu này hướng dẫn cách đóng gói các dịch vụ thành các container độc lập và kết nối chúng thành một hệ sinh thái hoàn chỉnh trên máy phát triển cục bộ bằng **Docker Compose**.

> [!IMPORTANT]
> **Quy định Phân định Ranh giới Repository**:
> - Tệp `docker/compose.demo.yml` trong repository này (`f-alumni-guideline`) **CHỈ LÀ BẢN TRÌNH DIỄN MẪU (Guideline Demo Only)** để minh họa cách tích hợp giữa Gateway, REST, gRPC, Kafka, SQL Server và Worker.
> - **Cấu hình Docker Compose và hạ tầng chính thức cho 5 microservices thực tế thuộc về repository độc lập: `fpt-alumni-infra`**.

---

## 🏗️ 2. Kiến trúc Mạng Container (Container-Facing vs Host-Facing Networking)

Một lỗi rất phổ biến của sinh viên khi mới làm quen với Docker là nhầm lẫn giữa địa chỉ kết nối **bên trong mạng Docker** và **từ máy chủ (Host)**:

```mermaid
flowchart TD
    subgraph HostMachine["Máy Tính Lập Trình Viên (Host Machine / Windows)"]
        Browser["Trình Duyệt Web / Postman\nhttp://localhost:5000\nSQL Server: localhost:14333\nKafka Host: localhost:29092"]
    end

    subgraph DockerBridge["Mạng Ảo Docker Bridge (f-alumni-demo-net)"]
        Gateway["api-gateway:5000"]
        Chat["chat-service:5002"]
        User["user-grpc-server:50051"]
        Worker["notification-worker"]
        KafkaBroker["kafka:9092"]
        SqlDb["sqlserver:1433"]
    end

    Browser -->|Port 5000| Gateway
    Browser -.->|Port 14333| SqlDb
    Browser -.->|Port 29092| KafkaBroker

    Gateway -->|Internal DNS: http://chat-service:5002| Chat
    Chat -->|Internal gRPC HTTP/2: http://user-grpc-server:50051| User
    Worker -->|Internal Kafka: kafka:9092| KafkaBroker
```

### 2.1. Bảng Phân bổ Địa chỉ Mạng & Cổng (Networking Matrix)

| Dịch vụ | Tên DNS Nội bộ (Container-facing) | Cổng Nội bộ | Cổng Ánh xạ Máy chủ (Host-facing) | Ghi chú & Mục đích |
| :--- | :--- | :---: | :---: | :--- |
| **YARP Gateway** | `api-gateway` | `5000` | **`localhost:5000`** | Cổng giao tiếp công khai duy nhất cho Web/Mobile. |
| **Chat REST API** | `chat-service` | `5002` | `localhost:5002` | Dịch vụ Chat nội bộ (được gọi qua Gateway). |
| **User gRPC Server** | `user-grpc-server` | `50051` | `localhost:50051` | Cung cấp gRPC HTTP/2 cho Chat Service tra cứu hồ sơ. |
| **Notification Worker** | `notification-worker` | N/A | N/A | Worker chạy ngầm, không mở cổng HTTP. |
| **Apache Kafka** | `kafka` | `9092` | **`localhost:29092`** | `9092` cho Worker nội bộ, `29092` cho app chạy trên host. |
| **SQL Server 2022** | `sqlserver` | `1433` | **`localhost:14333`** | Đổi sang `14333` để **không xung đột** với SQLEXPRESS (1433) trên máy thật. |

---

## ⚡ 3. Đảm bảo Giao tiếp gRPC HTTP/2 giữa các Container

gRPC yêu cầu giao thức **HTTP/2**. Mặc định, nếu kết nối HTTP không mã hóa (Cleartext / H2C), Kestrel cần được cấu hình tường minh để không bị hạ cấp xuống HTTP/1.1:

Trong `compose.demo.yml`:
```yaml
user-grpc-server:
  environment:
    ASPNETCORE_URLS: "http://+:50051"
    Kestrel__Endpoints__gRpc__Url: "http://+:50051"
    Kestrel__Endpoints__gRpc__Protocols: "Http2"
```

Nhờ cấu hình này, `chat-service` kết nối tới `http://user-grpc-server:50051` bên trong mạng bridge sẽ truyền tải các frame nhị phân Protobuf với tốc độ tối đa mà không gặp lỗi `Protocol error`.

---

## 🔐 4. Quản lý Biến Môi trường An toàn (`.env.example`)

Tuyệt đối không đưa mật khẩu thật vào tệp cấu hình được commit lên Git:
- **Tệp mẫu cam kết vào Git**: [`docker/.env.example`](file:///c:/Use/University/PRN232/ASM/guideline/docker/.env.example)
- **Tệp bí mật thực tế trên máy cá nhân**: `docker/.env` (đã được cấu hình trong `.gitignore`).

### Quy trình sử dụng:
1. Sao chép tệp mẫu:
   ```powershell
   Copy-Item docker/.env.example docker/.env
   ```
2. Mở `docker/.env` và chỉnh sửa mật khẩu SA hoặc các cổng mong muốn:
   ```ini
   MSSQL_SA_PASSWORD=YourStrong@Passw0rd!
   SQL_SERVER_HOST_PORT=14333
   GATEWAY_PORT=5000
   ```

---

## 🔄 5. Thứ tự Khởi động & Kiểm tra Tình trạng Sống (Healthchecks & Dependencies)

Tệp Compose sử dụng thuộc tính `depends_on` kết hợp với `condition: service_healthy`:
- **`chat-service`** chỉ khởi động sau khi **`user-grpc-server`** đã sẵn sàng.
- **`api-gateway`** chỉ khởi động sau khi **`chat-service`** đã sẵn sàng.
- **`notification-worker`** chỉ khởi động sau khi **`kafka`** đã vượt qua kiểm tra sức khỏe (`service_healthy` qua lệnh `kafka-topics.sh`).

---

## 💻 6. Lệnh Vận hành Cơ bản (Docker Operations)

### 6.1. Kiểm tra tính hợp lệ của cấu hình (Config Validation)
```powershell
docker compose -f docker/compose.demo.yml config
```
*Lệnh này phân tích cú pháp YAML, kiểm tra các biến môi trường và in ra toàn bộ cấu trúc định nghĩa dịch vụ.*

### 6.2. Khởi động toàn bộ hệ sinh thái demo
```powershell
docker compose -f docker/compose.demo.yml up -d --build
```

### 6.3. Theo dõi nhật ký hoạt động (Logs)
```powershell
# Xem log toàn bộ hệ thống
docker compose -f docker/compose.demo.yml logs -f

# Xem log riêng của Notification Worker để kiểm tra email mock
docker compose -f docker/compose.demo.yml logs -f notification-worker
```

### 6.4. Kiểm tra luồng End-to-End qua Gateway
Khi hệ thống đang chạy, bạn có thể gọi từ máy host vào cổng Gateway `5000`:
```powershell
# Tra cứu người dùng qua Gateway (Gateway -> Chat Service -> User gRPC)
curl http://localhost:5000/api/chat/conversations/11111111-1111-1111-1111-111111111111/participants/11111111-1111-1111-1111-111111111111
```

### 6.5. Dừng và dọn dẹp tài nguyên
```powershell
docker compose -f docker/compose.demo.yml down
```
