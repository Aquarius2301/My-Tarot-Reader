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
    tarot: {
      parentTitle: "Rút bài Tarot",
      title: "Rút 1 lá",
      intro:
        "Hãy để tâm trí thư thái, tập trung vào câu hỏi của bạn rồi chọn một lá bài.",
      yourCard: "Lá bài của bạn",
      upright: "Xuôi",
      reversed: "Ngược",
      drawAgain: "Rút lại",
      saving: "Đang lưu lá bài của bạn…",
      cooldown:
        "<strong>Lượt rút tiếp theo sẽ mở sau {{hours}} giờ {{minutes}} phút. <btn>Đăng nhập ngay</btn> để rút thêm bài.</strong>",
    },
    library: {
      title: "Kho bài Tarot",
      subtitle: "Tất cả 78 lá bài cùng ý nghĩa xuôi & ngược",
      tabMajor: "Bộ Ẩn Chính",
      tabMinor: "Bộ Ẩn Phụ",
      tabWands: "Gậy",
      tabCups: "Cốc",
      tabSwords: "Kiếm",
      tabPentacles: "Tiền",
      meaning: "Ý nghĩa của {{card}} · {{orientation}}",
    },
    history: {
      title: "Lịch sử trải bài",
      subtitle: "Xem lại các suy nghĩ và góc nhìn vũ trụ đã qua",
      empty: "Chưa có lượt trải bài nào",
      deleteDescription:
        "Trải bài này sẽ bị xóa vĩnh viễn. Bạn có chắc chắn muốn xóa không?",
      deleteTitle: "Xóa lượt trải bài này?",
      deleteConfirm: "Xóa lượt trải bài",
      deleteSuccess: "Đã xóa lượt trải bài",
    },
    login: {
      title: "Đăng nhập",
      heading: "Mở khóa trải nghiệm Tarot",
      subtitle:
        "Đăng nhập để lưu giữ thông điệp và kết nối sâu sắc hơn với năng lượng vũ trụ.",
      welcomeTitle: "Chào mừng bạn quay lại",
      welcomeSubtitle: "Đăng nhập nhanh chóng bằng tài khoản Google.",
      googleSignIn: "Đăng nhập với Google",
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
    streak: {
      title: "Điểm danh hàng ngày",
      subtitle:
        "Điểm danh mỗi ngày để nhận xu trắng và duy trì chuỗi năng lượng",
      currentStreak: "Chuỗi hiện tại",
      longestStreak: "Chuỗi dài nhất",
      days: "ngày",
      checkIn: "Điểm danh",
      checkedInToday: "Đã điểm danh hôm nay",
      checkInSuccess: "Điểm danh thành công",
      nextReward: "Phần thưởng ngày mai",
      saverUsed: "Đã dùng bảo vệ chuỗi tháng này",
      saverAvailable: "Còn lượt bảo vệ chuỗi tháng này",
      saverResetIn: "Còn {{days}} ngày nữa sẽ tự reset (00:00 mùng 1, giờ VN)",
      day1: "Ngày 1",
      day2: "Ngày 2",
      day3: "Ngày 3",
      day4: "Ngày 4",
      day5: "Ngày 5",
      day6: "Ngày 6",
      day7: "Ngày 7",
    },
  },
} as const;
