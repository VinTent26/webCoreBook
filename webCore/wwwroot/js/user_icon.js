document.addEventListener("DOMContentLoaded", function () {
    var userIcon = document.getElementById("userIcon");
    var userDropdownMenu = document.getElementById("userDropdownMenu");
    var profileLink = document.getElementById("profileLink");
    var logoutForm = document.getElementById("logoutForm");
    var loginLink = document.getElementById("loginLink");
    var registerLink = document.getElementById("registerLink");

    // Lấy trạng thái đăng nhập từ ViewBag
    var isLoggedIn = '@ViewBag.IsLoggedIn' === 'True';

    // Khi nhấn vào userIcon, toggle dropdown menu
    userIcon.addEventListener('click', function (e) {
        e.preventDefault(); // Ngừng sự kiện mặc định (nếu có)
        userDropdownMenu.classList.toggle("hidden");
    });

    // Kiểm tra trạng thái đăng nhập và hiển thị các mục phù hợp
    if (isLoggedIn) {
        profileLink.classList.remove("hidden");
        logoutForm.classList.remove("hidden");
        loginLink.classList.add("hidden");
        registerLink.classList.add("hidden");
    } else {
        profileLink.classList.add("hidden");
        logoutForm.classList.add("hidden");
        loginLink.classList.remove("hidden");
        registerLink.classList.remove("hidden");
    }
});