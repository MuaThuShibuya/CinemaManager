document.getElementById("momoPaymentBtn").addEventListener("click", function () {
    let btn = this;
    btn.disabled = true;
    document.getElementById("loading").classList.remove("hidden");

    let totalAmount = selectedSeats.length * 10000; // 🟢 Đảm bảo gửi số tiền chính xác

    fetch('/Booking/GetMomoQRCode', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            seatIds: selectedSeats,
            amount: totalAmount // 🟢 Gửi tổng tiền lên API
        })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                window.location.href = data.qrUrl;
            } else {
                showMessage("❌ Lỗi khi tạo QR thanh toán: " + data.error, "error");
            }
        })
        .catch(() => showMessage("❌ Lỗi hệ thống, vui lòng thử lại!", "error"))
        .finally(() => {
            btn.disabled = false;
            document.getElementById("loading").classList.add("hidden");
        });
});
