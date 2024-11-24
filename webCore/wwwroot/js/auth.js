document.addEventListener("DOMContentLoaded", function () {
    const loginForm = document.getElementById("loginForm"); // Đảm bảo form có id="loginForm"

    if (loginForm) {
        loginForm.addEventListener("submit", async function (event) {
            event.preventDefault(); // Ngăn việc reload trang

            const email = document.getElementById("email").value; // Lấy giá trị email
            const password = document.getElementById("password").value; // Lấy giá trị mật khẩu

            try {
                const response = await fetch('/User/Sign_in', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ email: email, password: password }),
                });

                const data = await response.json();

                if (response.ok && data.success) {
                    // Lưu UserToken vào sessionStorage
                    sessionStorage.setItem("UserToken", data.token);

                    // Chuyển hướng tới trang Home
                    window.location.href = '/Home/Index';
                } else {
                    // Hiển thị lỗi nếu đăng nhập thất bại
                    alert(data.message || "Đăng nhập thất bại. Vui lòng thử lại.");
                }
            } catch (error) {
                console.error("Đã xảy ra lỗi:", error);
                alert("Đã xảy ra lỗi khi đăng nhập. Vui lòng thử lại.");
            }
        });
    }
});