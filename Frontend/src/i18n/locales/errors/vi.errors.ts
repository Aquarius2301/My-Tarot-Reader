export const viErrors = {
  error: {
    server: {
      internalServerError: "Lỗi máy chủ nội bộ. Vui lòng thử lại sau.",
      badRequest: "Yêu cầu không hợp lệ. Vui lòng kiểm tra và thử lại.",
      unauthorized: "Bạn không được ủy quyền. Vui lòng đăng nhập để tiếp tục.",
      forbidden: "Bạn không có quyền truy cập tài nguyên này.",
      notFound: "Tài nguyên được yêu cầu không được tìm thấy.",
      conflict: "Xung đột với trạng thái hiện tại. Vui lòng thử lại sau.",
    },
    tarot: {
      drawnAlready:
        "Bạn đã rút một lá bài hôm nay. Vui lòng quay lại lúc 0 giờ",
    },
    aiTarot: {
      invalidCardCount: "Số lá bài không khớp với mức đã chọn.",
      invalidCard: "Một trong các lá bài không hợp lệ.",
      invalidConfig: "Không thể kết nối dịch vụ luận giải AI.",
      emptyAnswer: "AI không thể tạo câu trả lời. Vui lòng thử lại.",
    },
  },
};
