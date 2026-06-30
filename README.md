# Dự Án E-Commerce TechZone (ASP.NET Core + React)

Chào mừng bạn đến với TechZone - Hệ thống cửa hàng điện tử đa nền tảng hiện đại được xây dựng dựa trên công nghệ ASP.NET Core (Backend) và ReactJS (Frontend). Dự án tập trung vào việc đem lại trải nghiệm mua sắm mượt mà, chuyên nghiệp với giao diện người dùng (UI) và trải nghiệm người dùng (UX) đỉnh cao.

## 🚀 Tính năng nổi bật & Giao diện (UI/UX) chi tiết

Dự án TechZone được thiết kế không chỉ để chạy đúng logic mà còn chú trọng vào tính thẩm mỹ. Giao diện được tối ưu hóa theo phong cách "Dark Mode" (Nền tối kết hợp ánh sáng Neon) tương tự các hãng gaming/công nghệ lớn, tạo cảm giác sang trọng, cao cấp.

### 1. Trang Chủ (Home Page)
- **Banner Hero Động:** Băng chuyền hình ảnh (Carousel) nổi bật, tích hợp hiệu ứng gradient overlay và text bay lượn bắt mắt.
- **Danh sách sản phẩm:** Bố cục dạng lưới (Grid) hiện đại. Mỗi sản phẩm được bọc trong một "Card" sang trọng với:
  - Hiệu ứng *Hover* trượt nổi (Lift up) mượt mà khi người dùng di chuột vào.
  - Hình ảnh sản phẩm bo góc tinh tế.
  - Hiển thị đầy đủ thông tin: Tên sản phẩm, Giá bán (màu xanh Neon), số lượng tồn kho và Đánh giá sao trung bình.
- **Micro-interactions:** Nút "Thêm vào giỏ hàng" tích hợp hiệu ứng sáng lấp lánh (glow effect) kích thích tương tác.

### 2. Trang Chi tiết Sản phẩm & Đánh Giá (Reviews)
- **Hiển thị thông tin:** Chia layout thông minh với hình ảnh kích thước lớn bên trái và thông số kỹ thuật bên phải. 
- **Tính năng Đánh Giá (Review System):**
  - **Bảng điểm sao trung bình:** Tính toán điểm trung bình động từ cơ sở dữ liệu và hiển thị lớn nổi bật.
  - **Giao diện bình luận:** Các nhận xét của khách hàng được phân tách rõ ràng. Mỗi người dùng có một Avatar mặc định hoặc avatar cá nhân, thời gian đánh giá và nội dung chi tiết.
  - **Form gửi đánh giá:** Thiết kế theo dạng chọn sao tương tác trực quan. Đặc biệt, có kiểm tra trạng thái Đăng nhập. Nếu khách chưa đăng nhập, khung đánh giá sẽ ẩn đi và thay bằng nút Mời đăng nhập, giúp ngăn chặn Spam và đảm bảo dữ liệu thật.

### 3. Quy trình Thanh toán (Checkout & Payment)
- **Chọn Phương thức thanh toán:** Giao diện được thiết kế dạng các "Thẻ" (Cards) có hình ảnh icon sinh động thay vì các nút radio button nhàm chán truyền thống.
  - Người dùng có thể trực quan click chọn: Thanh toán khi nhận hàng (COD), Chuyển khoản (Momo), Thẻ tín dụng...
  - Thẻ được chọn sẽ có viền sáng Neon rực rỡ và dấu check xác nhận.
- **Trải nghiệm Mượt mà:** Toàn bộ form điền thông tin sử dụng phong cách *Floating Labels* của Bootstrap 5, trông cực kỳ gọn gàng và tinh tế.
- **Thông báo Toast:** Thay vì dùng popup Alert giật cục của trình duyệt, hệ thống sử dụng thư viện `react-toastify` để hiển thị các thông báo (thành công, lỗi) bay ra mượt mà từ góc màn hình.

### 4. Smart Chatbot (Trợ lý ảo Tự động)
- **Cửa sổ Chat Lơ Lửng (Floating Widget):** Một nút chat màu xanh lơ lửng ở góc dưới phải màn hình, đồng hành cùng khách hàng trên mọi trang.
- **Giao diện trò chuyện:** Khi bấm vào sẽ mở ra khung chat có thiết kế gần giống Messenger của Facebook. 
  - Khung chat bao gồm: Header thông tin Bot, vùng hiển thị tin nhắn (có phân biệt màu sắc giữa Bot và User), và ô nhập liệu.
  - **Hiệu ứng Typing:** Khi Bot đang xử lý, hệ thống sẽ hiện ra hiệu ứng 3 dấu chấm nhấp nháy, tạo cảm giác vô cùng chân thực.
- **Bộ não NLP (Keyword Matching) & AI Fallback:**
  - Chatbot có thể phân tích từ khóa của người dùng để trả lời ngay lập tức các vấn đề như: *Phí ship, bảo hành, giờ làm việc, thanh toán...*
  - Nếu câu hỏi quá phức tạp, Chatbot sẽ trả lời khéo léo và hướng dẫn khách hàng gọi Hotline.

### 5. Quản trị viên (Admin Dashboard - Backend)
- Giao diện Admin quản lý được thiết kế bằng MVC ASP.NET Core, tối ưu hóa bảng biểu để Admin dễ dàng xem, thêm, sửa, xóa sản phẩm, theo dõi đơn hàng và người dùng.

## 🛠 Công nghệ sử dụng
- **Backend:** C# ASP.NET Core Web API, Entity Framework Core, SQL Server.
- **Frontend:** ReactJS, Axios, Bootstrap 5, React-Toastify.
- **Khác:** JWT Authentication, RESTful API.

---
*(Nhánh BUOI07+08 - Hoàn thiện trải nghiệm mua sắm, Chatbot và Giao diện UI/UX)*
