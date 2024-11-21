document.addEventListener('DOMContentLoaded', () => {
    const innerUser = document.getElementById('userIcon');
    const userDropdownMenu = document.getElementById('userDropdownMenu');

    // Hiển thị hoặc ẩn menu khi nhấn vào inner-user
    innerUser.addEventListener('click', (e) => {
        e.preventDefault(); // Ngăn chặn hành vi mặc định
        userDropdownMenu.classList.toggle('hidden'); // Bật/tắt lớp 'hidden'
    });

    // Ẩn dropdown khi nhấp ra ngoài
    document.addEventListener('click', (e) => {
        if (!innerUser.contains(e.target) && !userDropdownMenu.contains(e.target)) {
            userDropdownMenu.classList.add('hidden'); // Ẩn menu
        }
    });
});