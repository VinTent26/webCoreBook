$(document).ready(function () {
    var categoryId = '@Html.Raw(ViewBag.CategoryId)'; ;

    // Kiểm tra nếu categoryId là giá trị hợp lệ
    if (categoryId) {
        console.log("Category ID:", categoryId);
    } else {
        console.log("Category ID is not set or is empty.");
    }

    // Khi nhấn nút khuyến mãi
    $('#sortDiscountBtn').click(function () {
        var sortOrder = $('#sortPriceSelect').val();
        loadProducts(categoryId, true, sortOrder);
    });

    // Khi thay đổi sắp xếp theo giá
    $('#sortPriceSelect').change(function () {
        var sortOrder = $(this).val();
        loadProducts(categoryId, true, sortOrder);
    });

    // Hàm tải sản phẩm
    function loadProducts(categoryId, filterByDiscount, sortOrder) {
        console.log("CategoryId trong hàm loadProducts:", categoryId); // Kiểm tra giá trị của categoryId khi gọi hàm

        $.ajax({
            url: '@Url.Action("GetProductsByCategoryId", "Product")',
            type: 'GET',
            data: {
                categoryId: categoryId,
                filterByDiscount: filterByDiscount,
                sortOrder: sortOrder
            },
            success: function (response) {
                $('#productRow').html(response);
            },
            error: function () {
                alert("Lỗi khi tải sản phẩm.");
            }
        });
    }
});