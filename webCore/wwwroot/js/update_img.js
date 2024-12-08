function previewImage(event) {
    const preview = document.getElementById('preview');
    const file = event.target.files[0];

    if (file) {
        // Tạo URL cho hình ảnh đã chọn
        const imageUrl = URL.createObjectURL(file);
        preview.src = imageUrl; // Cập nhật src cho hình ảnh preview
        preview.style.display = 'block'; // Hiện hình ảnh preview

        // Ẩn hình ảnh hiện tại nếu có
        const currentImage = document.getElementById('currentImage');
        if (currentImage) {
            currentImage.style.display = 'none'; // Ẩn hình ảnh cũ
        }

        // Giải phóng bộ nhớ khi hình ảnh đã được tải
        preview.onload = function () {
            URL.revokeObjectURL(imageUrl); // Giải phóng bộ nhớ
        }
    }
}
