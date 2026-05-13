/**
 * auth.js - Stub file. 
 * Phần đăng nhập demo đã được gỡ bỏ để sử dụng hệ thống xác thực JWT thực tế.
 */
window.SR_AUTH = {
    requireAuth: function(role, loginPath) {
        // Trả về true và ID giả lập để các trang demo vẫn hiển thị được dữ liệu mẫu
        // ID = 3 thường được dùng cho Nhà tuyển dụng trong demo-data.js
        // ID = 2 thường được dùng cho Ứng viên
        const id = (role === 'NhaTuyenDung' ? 3 : 2);
        return { maTaiKhoan: id, email: 'user@example.com', vaiTro: role };
    },
    updateNavPlaceholders: function() {
        // Không làm gì vì đã chuyển sang dùng Razor Navbar
    },
    getSession: function() {
        // Trả về object giả lập để không bị lỗi null reference trong một số script cũ
        return { maTaiKhoan: 3, email: 'user@example.com', vaiTro: 'NhaTuyenDung' };
    },
    logout: function() {
        // Việc đăng xuất thực tế được thực hiện qua Controller Auth/Logout
    }
};
