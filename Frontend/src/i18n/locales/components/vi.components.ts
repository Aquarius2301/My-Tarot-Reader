export const viComponents = {
  component: {
    mainLayout: {
      login: "Đăng nhập",
      logout: "Đăng xuất",
      logoutConfirm: "Bạn có chắc chắn muốn đăng xuất không?",
      theme: "Giao diện",
      language: "Ngôn ngữ",
      darkMode: "Chế độ tối",
      lightMode: "Chế độ sáng",
      guest: "Khách",
      vietnamese: "Tiếng Việt",
      english: "English",
    },
    deck: {
      instruction: "Di chuột để xem, nhấn để chọn lá bài",
      reshuffle: "Xáo bài lại",
      confirm: "Xác nhận",
      selected: "Đã chọn {{count}}/{{limit}}",
      limitReached: "Bạn chỉ được chọn tối đa {{n}} lá",
      noEnoughCoin: "Bạn không có đủ xu để rút bài này",
    },
    modal: {
      ok: "OK",
      cancel: "Hủy",
      delete: "Xóa",
    },
    copyButton: {
      copy: "Sao chép",
      copied: "Đã sao chép",
      failed: "Sao chép thất bại",
    },
    coin: {
      white: "Xu trắng",
      red: "Xu đỏ",
    },
    error: {
      offlineTitle: "Bạn đang ngoại tuyến",
      offlineDesc: "Vui lòng kiểm tra kết nối internet và thử lại.",
      serverTitle: "Lỗi máy chủ",
      serverDesc: "Đã xảy ra sự cố phía máy chủ. Vui lòng thử lại sau.",
      timeoutTitle: "Hết thời gian chờ kết nối",
      timeoutDesc:
        "Máy chủ phản hồi quá chậm. Vui lòng kiểm tra kết nối và thử lại.",
      genericTitle: "Đã xảy ra lỗi",
      genericDesc: "Có lỗi không xác định xảy ra. Vui lòng thử lại.",
      retry: "Thử lại",
    },
  },
} as const;
