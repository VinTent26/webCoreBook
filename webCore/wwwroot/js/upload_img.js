function previewImage(event) {
    const imagePreview = document.getElementById('imagePreview');
    const file = event.target.files[0];

    if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            imagePreview.src = e.target.result;
            imagePreview.style.display = 'block'; // Hiển thị ảnh xem trước
        }
        reader.readAsDataURL(file);
    } else {
        imagePreview.style.display = 'none'; // Ẩn ảnh nếu không có ảnh nào được chọn
    }
}