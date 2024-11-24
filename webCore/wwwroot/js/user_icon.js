document.addEventListener("DOMContentLoaded", function () {
    // Kiểm tra nếu thông tin người dùng có trong sessionStorage (đã đăng nhập)
    var UserToken = sessionStorage.getItem("UserToken");

    // Kiểm tra điều kiện đăng nhập và cập nhật UI khi tải trang
    if (UserToken) {
        // Hiển thị mục đăng xuất và thông tin cá nhân
        var profileLink = document.getElementById("profileLink");
        var logoutForm = document.getElementById("logoutForm");
        var loginLink = document.getElementById("loginLink");
        var registerLink = document.getElementById("registerLink");

        if (profileLink) profileLink.classList.remove("hidden");
        if (logoutForm) logoutForm.classList.remove("hidden");
        if (loginLink) loginLink.classList.add("hidden");
        if (registerLink) registerLink.classList.add("hidden");
    } else {
        // Nếu chưa đăng nhập, hiển thị mục đăng nhập và đăng ký, ẩn mục thông tin cá nhân và đăng xuất
        var profileLink = document.getElementById("profileLink");
        var logoutForm = document.getElementById("logoutForm");
        var loginLink = document.getElementById("loginLink");
        var registerLink = document.getElementById("registerLink");

        if (profileLink) profileLink.classList.add("hidden");
        if (logoutForm) logoutForm.classList.add("hidden");
        if (loginLink) loginLink.classList.remove("hidden");
        if (registerLink) registerLink.classList.remove("hidden");
    }

    // Đảm bảo dropdown menu ẩn khi mới tải trang
    const userDropdownMenu = document.getElementById("userDropdownMenu");
    if (userDropdownMenu) {
        userDropdownMenu.classList.add("hidden");
    }

    // Kiểm tra phần tử button có tồn tại không
    const userMenuButton = document.getElementById("userIcon");
    if (userMenuButton) {
        userMenuButton.addEventListener("click", function () {
            if (userDropdownMenu) {
                // Toggle trạng thái ẩn/hiện của dropdown menu
                userDropdownMenu.classList.toggle("hidden");
            }
        });
    } else {
        console.error("Nút userIcon không tồn tại.");
    }

    // Xử lý sự kiện đăng xuất
    const logoutButton = document.getElementById("logoutButton");
    if (logoutButton) {
        logoutButton.addEventListener("click", function () {
            // Xóa token khỏi sessionStorage
            sessionStorage.removeItem("UserToken");

            // Tải lại trang (hoặc chuyển hướng nếu cần)
            location.reload(); // Hoặc window.location.href = "/User/Sign_in" nếu bạn muốn chuyển hướng đến trang đăng nhập.
        });
    }
});
