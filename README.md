# Tiêu Chí Đánh Giá & Xây Dựng Trang Web Thương Mại Điện Tử (TechZone)

Dự án TechZone (ASP.NET Core + ReactJS) được phát triển không chỉ để đáp ứng các tính năng cơ bản của một hệ thống bán hàng, mà còn tuân thủ nghiêm ngặt các tiêu chuẩn và tiêu chí khắt khe của một trang web chuyên nghiệp trong ngành công nghiệp phần mềm hiện đại.

Dưới đây là tổng quát các tiêu chí cốt lõi được áp dụng trong quá trình làm trang web:

## 1. Tiêu chí về Giao diện & Trải nghiệm Người dùng (UI/UX)
- **Tính Thẩm mỹ (Aesthetics):** Sử dụng ngôn ngữ thiết kế hiện đại, phối màu Dark Theme (Nền tối - chữ sáng) sang trọng, mang phong cách công nghệ cao (High-tech) thu hút người dùng.
- **Tính Nhất quán (Consistency):** Đồng bộ màu sắc, typography (font chữ), icon và khoảng cách (padding/margin) xuyên suốt toàn bộ ứng dụng thông qua bộ Design System định sẵn.
- **Tính Phản hồi tương tác (Micro-interactions):** Mọi thao tác click, hover, thêm vào giỏ hàng đều có hiệu ứng mượt mà (animation/transition), giúp người dùng nhận thức rõ ràng hành động của mình.
- **Đơn giản hóa quy trình (Simplicity):** Tối ưu hóa số bước thanh toán (Checkout) và đăng ký/đăng nhập. Sử dụng Floating labels và Toast notifications thay cho Popup rườm rà.

## 2. Tiêu chí về Hiệu năng (Performance & Scalability)
- **Tốc độ tải trang:** Sử dụng ReactJS để thiết kế theo kiến trúc Single Page Application (SPA), giúp trang web chỉ tải 1 lần đầu tiên và sau đó chuyển trang mượt mà không cần load lại toàn bộ trình duyệt.
- **Xử lý Bất đồng bộ (Asynchronous):** Toàn bộ các tương tác với cơ sở dữ liệu qua ASP.NET Core API đều sử dụng `async/await` để không làm nghẽn luồng xử lý (non-blocking).
- **Thiết kế CSDL Tối ưu (Database Design):** Sử dụng Entity Framework Core với kiến trúc bảng hợp lý, đánh Index đầy đủ, có khóa ngoại (Foreign Keys) chặt chẽ giữa Customer, Product, Order, Review.

## 3. Tiêu chí về Bảo mật (Security)
- **Xác thực & Phân quyền (Authentication & Authorization):** Sử dụng công nghệ JWT (JSON Web Token) tân tiến kết hợp Cookie an toàn (HttpOnly) để bảo mật phiên đăng nhập, chống lại các cuộc tấn công CSRF, XSS.
- **Bảo mật Dữ liệu nhạy cảm:** Mật khẩu người dùng được băm (Hash) một chiều trước khi lưu vào cơ sở dữ liệu. API Keys (Ví dụ: Gemini AI, Mail) được đưa ra khỏi mã nguồn để ngăn rò rỉ.
- **Xác thực Đầu vào (Input Validation):** Kiểm tra chặt chẽ dữ liệu người dùng nhập từ cả 2 phía: Frontend (React Hook Form/Yup) và Backend (Data Annotations trong C#) nhằm ngăn chặn SQL Injection.

## 4. Tiêu chí về Tính Tương thích & Tối ưu (Responsive & SEO)
- **Mobile First / Responsive Design:** Hệ thống được xây dựng tương thích hoàn hảo trên mọi kích thước màn hình (Điện thoại, Tablet, Desktop) nhờ vào hệ thống Grid của Bootstrap 5 và CSS Flexbox/Grid.
- **Tối ưu Hóa Tìm Kiếm (SEO):** Đảm bảo cấu trúc thẻ HTML (H1, H2, H3), meta description đầy đủ để thân thiện với các công cụ tìm kiếm của Google.

## 5. Tiêu chí về Công nghệ Mới & Đột phá (Innovation)
- **Tích hợp Trí Tuệ Nhân Tạo (AI Chatbot):** Trang bị trợ lý ảo thông minh tự động (Sử dụng Google Gemini API) có khả năng hiểu ngữ cảnh và trả lời tự nhiên 24/7.
- **Hệ thống Dự phòng (Fallback System):** Khi API bên thứ 3 gặp sự cố, hệ thống có khả năng tự động chuyển đổi (Switch) về chế độ Keyword NLP cục bộ, đảm bảo tính liên tục của dịch vụ.

## 6. Tiêu chí về Bảo trì & Mở rộng (Maintainability)
- **Kiến trúc Phân tầng (Layered Architecture):** Code Backend được chia tách rõ ràng thành Controllers (Xử lý Request), Services (Xử lý nghiệp vụ logic), Data/Entities (Tương tác CSDL).
- **Clean Code:** Viết code tuân thủ nguyên tắc SOLID, mã nguồn sạch sẽ, dễ đọc, có chú thích (Comments) đầy đủ ở những hàm phức tạp, giúp các lập trình viên khác dễ dàng tiếp quản và phát triển tính năng mới.

---
*(Nhánh BUOI09+10 - Định chuẩn và Tiêu chí Đánh giá Dự án Phần mềm)*
