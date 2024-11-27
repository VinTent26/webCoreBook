document.addEventListener('DOMContentLoaded', function () {
    document.querySelector('#addToCartButton').addEventListener('click', function () {
        // Lấy các dữ liệu từ thuộc tính data của nút
        const productId = this.getAttribute('data-product-id');
        const title = this.getAttribute('data-title');
        const price = parseFloat(this.getAttribute('data-price'));
        const image = this.getAttribute('data-image');
        const quantity = 1; // Bạn có thể thay đổi cách lấy số lượng nếu muốn

        // Gọi hàm để thêm vào giỏ hàng
        addToCart(productId, title, price, quantity, image);
    });
});

// Hàm thêm sản phẩm vào giỏ hàng
function addToCart(productId, title, price, quantity, image) {
    $.ajax({
        url: '@Url.Action("AddToCart", "Cart")',  // Đảm bảo URL trỏ đúng đến action AddToCart
        type: 'POST',
        data: {
            productId: productId,
            title: title,
            price: price,
            quantity: quantity,
            image: image
        },
        success: function (response) {
            alert('Sản phẩm đã được thêm vào giỏ hàng!');
        },
        error: function () {
            alert('Có lỗi xảy ra khi thêm vào giỏ hàng');
        }
    });
}