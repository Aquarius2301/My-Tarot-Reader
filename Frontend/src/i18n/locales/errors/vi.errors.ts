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
    aiDeepTarot: {
      invalidCardCount: "Số lá bài không hợp lệ.",
      invalidCard: "Một hoặc nhiều lá bài không hợp lệ.",
      invalidLocale: "Ngôn ngữ không được hỗ trợ.",
      invalidQuestion: "Câu hỏi của bạn không hợp lệ.",
      invalidOption: "Các lựa chọn không hợp lệ.",
      invalidTimeFrame: "Khung thời gian không được hỗ trợ.",
      unsafeContent:
        "Trải bài không thể đọc cho nội dung này. Nếu bạn đang có suy nghĩ tự làm hại mình, hãy nói chuyện với người bạn tin tưởng hoặc liên hệ đường dây nóng hỗ trợ khủng hoảng tại nơi bạn sống.",
      questionNotSupported:
        "Câu hỏi của bạn chưa phải là một quyết định có thể đọc trải bài. Vui lòng mô tả rõ lựa chọn bạn đang phân vân bằng ngôn ngữ của bạn.",
      readingNotFound: "Không tìm thấy trải bài chuyên sâu này.",
      generationFailed:
        "Không thể tạo trải bài chuyên sâu. Vui lòng thử lại.",
    },
    wallet: {
      insufficientCoins: "Bạn không đủ xu trắng để thực hiện hành động này.",
      insufficientRedCoin: "Bạn không đủ xu đỏ để thực hiện hành động này.",
      invalidAmount: "Số xu không hợp lệ.",
      walletNotFound: "Không tìm thấy ví của bạn.",
    },
    shop: {
      invalidPackage: "Gói này không khả dụng.",
      orderNotFound: "Không tìm thấy đơn hàng.",
      createPaymentFailed: "Không thể tạo giao dịch. Vui lòng thử lại.",
      invalidWebhookSignature: "Chữ ký thông báo thanh toán không hợp lệ.",
      confirmWebhookFailed: "Không thể xác nhận thanh toán. Vui lòng thử lại.",
      payOsError:
        "Nhà cung cấp thanh toán báo lỗi. Vui lòng thử lại.",
    },
  },
};
