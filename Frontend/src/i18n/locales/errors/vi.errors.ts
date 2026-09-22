export const viErrors = {
  error: {
    system: {
      internalServerError: "Lỗi máy chủ nội bộ. Vui lòng thử lại sau.",
      badRequest: "Yêu cầu không hợp lệ. Vui lòng kiểm tra và thử lại.",
      unauthorized: "Bạn không được ủy quyền. Vui lòng đăng nhập để tiếp tục.",
      forbidden: "Bạn không có quyền truy cập tài nguyên này.",
      notFound: "Tài nguyên được yêu cầu không được tìm thấy.",
      conflict: "Xung đột với trạng thái hiện tại. Vui lòng thử lại sau.",
    },
    tarot: {
      drawnAlready: "Bạn đã rút một lá bài cho trải bài này.",
      notFound: "Không tìm thấy trải bài này.",
    },
    streak: {
      alreadyCheckedIn: "Bạn đã điểm danh hôm nay rồi.",
    },
    aiTarot: {
      invalidCardCount: "Số lá bài không hợp lệ.",
      invalidCard: "Một hoặc nhiều lá bài không hợp lệ.",
      invalidLocale: "Ngôn ngữ không được hỗ trợ.",
      readingNotFound: "Không tìm thấy trải bài AI này.",
      generationFailed: "Không thể tạo trải bài AI. Vui lòng thử lại.",
    },
    wallet: {
      insufficientCoins: "Bạn không đủ xu trắng để thực hiện hành động này.",
    },
  },
};
