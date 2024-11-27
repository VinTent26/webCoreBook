document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.btn-minus').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const input = this.nextElementSibling; // Lấy phần tử input sau nút trừ
            let value = parseInt(input.value) || 1;
            if (value > 1) {
                input.value = value - 1; // Giảm số lượng
            }
            // Lấy giá trị mới sau khi thay đổi
            let updatedQuantity = parseInt(input.value);
            console.log("Số lượng sau khi thay đổi: " + updatedQuantity); // In ra số lượng đã thay đổi
        });
    });

    document.querySelectorAll('.btn-plus').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const input = this.previousElementSibling; // Lấy phần tử input trước nút cộng
            let value = parseInt(input.value) || 1;
            input.value = value + 1; // Tăng số lượng
            // Lấy giá trị mới sau khi thay đổi
            let updatedQuantity = parseInt(input.value);
            console.log("Số lượng sau khi thay đổi: " + updatedQuantity); // In ra số lượng đã thay đổi
        });
    });
});
