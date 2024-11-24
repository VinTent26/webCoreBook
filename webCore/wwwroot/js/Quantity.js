document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.btn-minus').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const input = this.nextElementSibling;
            let value = parseInt(input.value) || 1;
            if (value > 1) {
                input.value = value - 1;
            }
        });
    });

    document.querySelectorAll('.btn-plus').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const input = this.previousElementSibling;
            let value = parseInt(input.value) || 1;
            input.value = value + 1;
        });
    });
});