# 🚀 HỆ THỐNG QUẢN TRỊ NỘI DUNG (CMS) & BÁN HÀNG TỔNG HỢP

Chào mừng bạn đến với dự án **Hệ thống Quản trị Nội dung (CMS) & E-Commerce** được xây dựng trên nền tảng **ASP.NET Core** kết hợp với **ReactJS**. Dự án này được thiết kế theo kiến trúc chuẩn hiện đại, phân tách rõ ràng giữa Backend (API & Admin Dashboard) và Frontend (Client Application).

---

## 📌 1. TỔNG QUAN DỰ ÁN (OVERVIEW)

Dự án này là một giải pháp toàn diện hỗ trợ doanh nghiệp vừa và nhỏ trong việc:
- **Quản lý nội dung (CMS):** Tạo, chỉnh sửa, xuất bản các bài viết tin tức, quản lý danh mục bài viết (Category Post).
- **Quản lý bán hàng (E-Commerce):** Quản lý sản phẩm, danh mục sản phẩm, theo dõi đơn đặt hàng (Orders) và thông tin khách hàng (Customers).
- **Trải nghiệm khách hàng:** Cung cấp giao diện người dùng (Frontend) mượt mà với React, hỗ trợ giỏ hàng, thanh toán và xem tin tức.

### Công nghệ sử dụng:
- **Backend:** ASP.NET Core 8.0 (MVC cho trang Admin & Web API cho Frontend)
- **Database:** Microsoft SQL Server (sử dụng Entity Framework Core - Code First)
- **Frontend:** ReactJS (React Router, Axios, Bootstrap 5)
- **Công cụ khác:** CKEditor (soạn thảo văn bản), Swagger (Document API)

---

## 🏗️ 2. KIẾN TRÚC HỆ THỐNG

Dự án được chia thành 3 lớp (Layers) chính để đảm bảo tính module hóa và dễ bảo trì:

1. **CMS.DATA:** Lớp dữ liệu (Data Access Layer) chứa `ApplicationDbContext` và các thực thể (Entities) như `Post`, `Product`, `Order`, `Customer`, `User`. Chịu trách nhiệm tương tác trực tiếp với cơ sở dữ liệu SQL Server.
2. **CMS.Backend:** Dự án chính (Presentation Layer cho Admin và API Layer cho Frontend). Bao gồm:
   - Các `Controllers` trả về View (HTML/CSS) cho trang quản trị Admin.
   - Các `ApiControllers` trả về dữ liệu JSON để giao tiếp với React Frontend.
   - Quản lý xác thực (Authentication), phân quyền (Authorization) và xử lý file tĩnh (wwwroot).
3. **cms.frontend:** Ứng dụng Client được xây dựng bằng ReactJS. Gọi API từ Backend để hiển thị danh sách sản phẩm, tin tức, xử lý giỏ hàng và thanh toán.

---

## 🚀 3. HƯỚNG DẪN CÀI ĐẶT & CHẠY DỰ ÁN TỪ A-Z

Để có thể chạy được dự án này trên máy cá nhân, vui lòng làm theo các bước thật chi tiết dưới đây.

### BƯỚC 1: Chuẩn bị môi trường (Prerequisites)
- Cài đặt [Visual Studio 2022](https://visualstudio.microsoft.com/) (Hỗ trợ .NET 8.0 SDK).
- Cài đặt [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) hoặc SQL Server Express.
- Cài đặt [Node.js](https://nodejs.org/en/) (phiên bản LTS) để chạy React Frontend.

### BƯỚC 2: Cấu hình Cơ sở dữ liệu (Database Setup)
1. Mở file `appsettings.json` trong thư mục `CMS.Backend`.
2. Tìm chuỗi kết nối (Connection String) `DefaultConnection`.
3. Thay đổi thông tin Server (`Server=...`) thành Tên Server SQL của máy bạn.
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=CMS_Db;Trusted_Connection=True;TrustServerCertificate=True"
    }
    ```
4. Mở **Package Manager Console** trong Visual Studio (vào `Tools` > `NuGet Package Manager` > `Package Manager Console`).
5. Chọn `Default project` là `CMS.DATA`.
6. Chạy lệnh: `Update-Database` để tự động tạo cơ sở dữ liệu `CMS_Db` và các bảng cần thiết.

### BƯỚC 3: Khởi chạy Backend (Admin & API)
1. Trong Visual Studio, thiết lập `CMS.Backend` làm dự án khởi chạy mặc định (Set as Startup Project).
2. Nhấn `F5` hoặc nút Play để chạy Backend.
3. Trang quản trị Admin sẽ hiển thị tại địa chỉ: `https://localhost:7222/` (port có thể thay đổi tùy cấu hình của bạn).
4. Bạn có thể truy cập `https://localhost:7222/swagger` để xem tài liệu API.

### BƯỚC 4: Khởi chạy Frontend (ReactJS)
1. Mở Terminal hoặc Command Prompt, trỏ đường dẫn vào thư mục `cms.frontend`.
2. Chạy lệnh cài đặt các thư viện phụ thuộc:
    ```bash
    npm install
    ```
3. Sau khi cài đặt xong, khởi chạy ứng dụng Frontend bằng lệnh:
    ```bash
    npm start
    ```
4. Trình duyệt sẽ tự động mở trang web giao diện khách hàng tại địa chỉ `http://localhost:3000/`.

---

## 🎯 4. CÁC TÍNH NĂNG NỔI BẬT

### 4.1. Hệ thống Quản trị (Admin Dashboard)
- **Bảng điều khiển chuyên nghiệp:** Thống kê tổng doanh thu, số lượng đơn hàng, sản phẩm và bài viết mới nhất trực quan.
- **Quản lý Sản phẩm đa dạng:** Cho phép thêm mới, sửa, xóa sản phẩm. Hỗ trợ upload ảnh sản phẩm, thêm nhiều ảnh chi tiết (Product Images gallery).
- **Quản lý Bài viết & Danh mục:** Trình soạn thảo văn bản phong phú (Rich-text editor) cho phép nhúng ảnh trực tiếp vào bài viết.
- **Quản lý Đơn hàng:** Xem chi tiết thông tin đơn hàng, thông tin người nhận và thay đổi trạng thái giao hàng.

### 4.2. Giao diện Người dùng (Frontend React)
- **Giỏ hàng thông minh:** Thêm sản phẩm vào giỏ hàng, thay đổi số lượng, tự động tính tổng tiền (sử dụng React Context API).
- **Tính năng Thanh toán (Checkout):** Lưu thông tin người nhận hàng vào LocalStorage để tiện lợi cho các lần mua sau.
- **Tin tức & Blog:** Đọc tin tức công nghệ, tự động load ảnh động cho dù là ảnh Thumbnail hay ảnh bên trong bài viết (Tương thích đường dẫn URL tuyệt đối).

---

## 🛠️ 5. LƯU Ý KHI SỬ DỤNG
- **Upload Ảnh:** Khi upload ảnh sản phẩm hay bài viết, ảnh sẽ được lưu vật lý trong thư mục `wwwroot/images/uploads` của `CMS.Backend`. Vui lòng không xóa thư mục này.
- **Lỗi hiển thị ảnh trên Frontend:** Nếu Frontend không hiện ảnh, hãy chắc chắn rằng Backend đang chạy và tham số `IMAGE_BASE_URL` trong React (file `.env` hoặc cấu hình cố định) trỏ đúng tới địa chỉ cổng của Backend (ví dụ `https://localhost:7222`).

---
_Chúc bạn có một trải nghiệm tuyệt vời với dự án này! Mọi đóng góp xin vui lòng tạo Pull Request vào nhánh chính._
