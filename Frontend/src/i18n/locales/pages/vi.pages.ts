export const viPages = {
  page: {
    home: {
      title: "Trang chủ",
      heroTitle: "Lắng Nghe Thông Điệp Từ",
      heroTitleHighlight: "Vũ Trụ & Những Lá Bài",
      heroDescription:
        "Giải mã vận mệnh, tình yêu, và sự nghiệp với công nghệ phân tích Tarot kết hợp AI. Nhận câu trả lời chính xác và lời khuyên chữa lành tâm hồn ngay lập tức.",
      heroDrawCard: "Rút Bài Hôm Nay",
      heroUpgradePro: "Nâng Cấp PRO",
      heroLoginNow: "Đăng nhập ngay",
    },
    login: {
      title: "Đăng nhập",
      heading: "Mở khóa trải nghiệm Tarot",
      subtitle:
        "Đăng nhập để lưu giữ thông điệp và kết nối sâu sắc hơn với năng lượng vũ trụ.",
      welcomeTitle: "Chào mừng bạn quay lại",
      welcomeSubtitle: "Đăng nhập nhanh chóng bằng tài khoản Google.",
      googleLoginError: "Đăng nhập Google thất bại. Vui lòng thử lại.",
      benefit: {
        ai: {
          title: "Giải bài chuyên sâu cùng AI",
          description:
            "Nhận luận giải cá nhân hóa theo từng câu hỏi và bối cảnh tâm lý của bạn.",
        },
        history: {
          title: "Lưu trữ lịch sử trải bài",
          description:
            "Xem lại toàn bộ các lá bài đã rút và hành trình năng lượng theo thời gian.",
        },
        daily: {
          title: "Thống kê năng lượng hàng ngày",
          description:
            "Nhận thông điệp Tarot đầu ngày và đề xuất cân bằng cảm xúc.",
        },
      },
    },
    draw: {
      title: "Rút bài Tarot",
      oneCard: { title: "Rút 1 lá" },
    },
    history: {
      title: "Lịch sử trải bài",
      subtitle: "Xem lại các suy nghĩ và góc nhìn vũ trụ đã qua",
      empty: "Chưa có lịch sử trải bài",
      reversed: "Ngược",
      upright: "Xuôi",
      deleteAria: "Xóa lượt trải bài {{card}}",
      deleteConfirmTitle: "Xóa lượt trải bài này?",
      deleteOk: "Xóa",
      deleteCancel: "Hủy",
      deleteSuccess: "Đã xóa lượt trải bài",
    },
    aiDraw: {
      title: "Xem bài cùng AI",
      intro:
        "Chọn số lá, loại câu hỏi và ngôn ngữ, sau đó rút bài để nhận luận giải chuyên sâu từ AI.",
      languageLabel: "Ngôn ngữ luận giải",
      language: {
        vi: "Tiếng Việt",
        en: "English",
      },
      cardPositionsLabel: "Vị trí của các lá",
      cardPositions: {
        three: "Năng lượng cốt lõi, Thử thách / vật cản, Kết quả / lời khuyên",
        five: "Năng lượng cốt lõi, Thử thách / vật cản, Điểm mạnh của bạn, Tương lai, Kết quả / lời khuyên",
        seven:
          "Năng lượng cốt lõi, Thử thách / vật cản, Điểm mạnh của bạn, Ảnh hưởng tiềm ẩn, Cách đối mặt, Tương lai, Kết quả / lời khuyên",
        ten: "Năng lượng cốt lõi, Thử thách / vật cản, Điều cần tập trung, Quá khứ, Điểm mạnh của bạn, Tương lai gần, Cách tiếp cận đề xuất, Điều bạn cần biết, Hy vọng / nỗi sợ, Kết quả / lời khuyên",
      },
      confirmButton: "Xem bài",
      saving: "Đang luận giải...",
      resultTitle: "Kết quả luận giải",
      yourCards: "Các lá bài của bạn",
      drawAgain: "Rút lại",
      invalidSelection: "Vui lòng chọn đủ số lá bài trước khi xem bài.",
    },
  },
  nav: {
    login: "Đăng nhập",
    logout: "Đăng xuất",
    theme: "Giao diện",
    language: "Ngôn ngữ",
    darkMode: "Chế độ tối",
    lightMode: "Chế độ sáng",
    guest: "Khách",
    vietnamese: "Tiếng Việt",
    english: "English",
  },
  select: {
    questionTypeLabel: "Loại câu hỏi",
    questionType: {
      energy: "Năng lượng",
      love: "Tình yêu",
      career: "Sự nghiệp",
      money: "Tiền bạc",
    },
    cardCountLabel: "Số lá bài",
    cardCount: {
      three: "3 lá",
      five: "5 lá",
      seven: "7 lá",
      ten: "10 lá",
    },
  },
} as const;
