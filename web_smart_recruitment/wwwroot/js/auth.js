/**
 * auth.js - Stub file. 
 * Phần đăng nhập demo đã được gỡ bỏ để sử dụng hệ thống xác thực JWT thực tế.
 */
window.SR_AUTH = {
    requireAuth: function(role, loginPath) {
        // Chức năng này hiện đã được thay thế bởi [Authorize] attribute ở phía Server.
        // Trả về null để yêu cầu script phía client kiểm tra lại trạng thái thực tế.
        return null;
    },
    updateNavPlaceholders: function() {
        // Navbar hiện tại được render từ phía Server (Razor), không cần xử lý JS.
    },
    getSession: function() {
        // Trả về null để đồng bộ với việc đăng xuất thực tế trên Server.
        // Các script phía client sẽ nhận diện là chưa đăng nhập.
        return null;
    },
    logout: function() {
        // Chuyển hướng đến controller đăng xuất thực tế trên Server
        window.location.href = '/Auth/Logout';
    }
};
