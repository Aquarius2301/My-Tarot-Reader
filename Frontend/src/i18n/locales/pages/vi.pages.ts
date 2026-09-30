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
      streakTitle: "Điểm danh hàng ngày",
      streakSubtitle:
        "Điểm danh mỗi ngày để nhận xu trắng và duy trì chuỗi năng lượng",
      streakCurrentStreak: "Chuỗi hiện tại",
      streakLongestStreak: "Chuỗi dài nhất",
      streakDays: "ngày",
      streakCheckIn: "Điểm danh",
      streakCheckedInToday: "Đã điểm danh hôm nay",
      streakCheckInSuccess: "Điểm danh thành công",
      streakReward: "Phần thưởng hôm nay",
      streakSaverUsed: "Đã dùng bảo vệ chuỗi tháng này",
      streakSaverAvailable: "Còn lượt bảo vệ chuỗi tháng này",
      streakSaverResetIn:
        "Còn {{days}} ngày nữa sẽ tự reset (00:00 mùng 1, giờ VN)",
      streakDay1: "Ngày 1",
      streakDay2: "Ngày 2",
      streakDay3: "Ngày 3",
      streakDay4: "Ngày 4",
      streakDay5: "Ngày 5",
      streakDay6: "Ngày 6",
      streakDay7: "Ngày 7",
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
    aiTarot: {
      title: "AI Tarot",
      step2: {
        subtitle:
          "Hãy để tâm trí thư thái, tập trung vào câu hỏi rồi chọn {{count}} lá bài.",
      },
      cardCount: "Số lá bài",
      cardCountOption: "{{count}} lá",
      positions: "Các vị trí của trải bài",
      questionType: "Chủ đề câu hỏi",
      questionTypes: {
        energy: "Năng lượng",
        love: "Tình cảm",
        career: "Sự nghiệp",
        money: "Tài chính",
      },
      cost: "Chi phí: {{cost}} xu trắng",
      balance: "Xu trắng hiện có: {{balance}}",
      insufficientCoins:
        "Bạn cần tối thiểu {{cost}} xu trắng để tiếp tục. Hãy điểm danh hằng ngày để nhận thêm xu.",
      continue: "Tiếp tục",
      back: "Quay lại",
      saving: "Đang tạo trải bài AI của bạn…",
      position: {
        coreEnergy: "Năng lượng cốt lõi",
        challenges: "Thử thách / trở ngại",
        outcome: "Kết quả / lời khuyên",
        yourStrength: "Sức mạnh của bạn",
        future: "Tương lai",
        hiddenInfluences: "Ảnh hưởng tiềm ẩn",
        wayToFace: "Cách đối mặt",
        focus: "Điều cần tập trung",
        past: "Quá khứ",
        nearFuture: "Tương lai gần",
        approach: "Hướng xử lý đề xuất",
        needToKnow: "Điều bạn cần biết",
        hopesFears: "Hy vọng / lo sợ",
      },
      result: {
        title: "Kết quả giải bài",
        overview: "Tổng quan",
        advice: "Lời khuyên tổng thể",
        noAnswer: "Đang cập nhật nội dung giải bài.",
        drawAgain: "Rút bài mới",
      },
    },
    aiDeepTarot: {
      parentTitle: "Tarot chuyên sâu",
      common: {
        cardCount: "Số lá bài: {{count}} lá · {{positions}} vị trí",
        cost: "Chi phí: {{cost}} xu đỏ",
        balance: "Xu đỏ hiện có: {{balance}}",
        insufficientCoins:
          "Bạn cần tối thiểu {{cost}} xu đỏ để tiếp tục. Hãy điểm danh hằng ngày để nhận thêm xu.",
        continue: "Tiếp tục",
        back: "Quay lại",
        saving: "Đang tạo trải bài chuyên sâu của bạn…",
      },
      result: {
        title: "Kết quả giải bài chuyên sâu",
        overview: "Tổng quan",
        advice: "Lời khuyên tổng thể",
        noAnswer: "Đang cập nhật nội dung giải bài.",
        drawAgain: "Rút bài mới",
      },
      spreads: {
        twelveHouses: {
          title: "Trải bài 12 nhà",
          subtitle:
            "12 lá bài lần lượt ngồi vào từng ngôi nhà, phản ánh năng lượng và trạng thái hiện tại của bạn ở từng khía cạnh.",
          what: {
            title: "12 Nhà trong Tarot là gì?",
            body: "Trong Chiêm tinh, cuộc sống của mỗi người được chia thành 12 mảnh ghép đại diện cho 12 lĩnh vực: từ bản thân, tiền bạc, tình cảm, gia đình cho đến sự nghiệp và tâm linh. Khi chọn trải bài 12 Nhà, các lá Tarot sẽ lần lượt ngồi vào từng ngôi nhà này để phản ánh năng lượng và trạng thái hiện tại của bạn ở từng khía cạnh.",
          },
          why: {
            title: "Trải bài này để làm gì?",
            items: {
              overview:
                "Cung cấp góc nhìn toàn cảnh: Thay vì chỉ trả lời một câu hỏi lẻ (như \"Người ấy có thích tôi không?\"), trải bài này giúp bạn thấy được bức tranh lớn về mọi mặt trong đời sống.",
              blockage:
                "Tìm ra điểm nghẽn: Nhận diện nhanh lĩnh vực nào đang phát triển thuận lợi và khía cạnh nào đang gặp rắc rối cần bạn tập trung xử lý.",
              forecast:
                "Dự báo năng lượng: Giúp bạn chuẩn bị tâm lý và hướng đi phù hợp cho các sự kiện sắp tới.",
            },
          },
          when: {
            title: "Khi nào bạn nên xem 12 Nhà?",
            items: {
              milestone:
                "Vào các cột mốc mới: Dịp đầu năm mới, sinh nhật, hoặc khởi đầu một chu kỳ mới trong cuộc sống.",
              lost:
                "Khi mất phương hướng: Bạn cảm thấy mọi thứ không ổn nhưng không chỉ ra được cụ thể vấn đề nằm ở đâu (là do công việc, tình cảm, hay sức khỏe tinh thần).",
              selfReview:
                "Khi muốn đánh giá lại bản thân: Dành cho những lúc bạn muốn dừng lại, soi rọi lại toàn bộ cuộc sống để lên kế hoạch cân bằng lại mọi thứ.",
            },
          },
          positions: {
            title: "12 ngôi nhà trong trải bài của bạn",
            hint: "Mỗi lá bài bạn chọn sẽ được giải nghĩa theo đúng ngôi nhà tương ứng.",
          },
          position: {
            "house-1": "Nhà 1 · Bản thân & danh tính",
            "house-2": "Nhà 2 · Tiền bạc & giá trị bản thân",
            "house-3": "Nhà 3 · Tư duy & giao tiếp",
            "house-4": "Nhà 4 · Gia đình & cội nguồn",
            "house-5": "Nhà 5 · Sáng tạo & niềm vui",
            "house-6": "Nhà 6 · Sức khỏe & đời sống hằng ngày",
            "house-7": "Nhà 7 · Đối tác & hôn nhân",
            "house-8": "Nhà 8 · Thay đổi & thân mật",
            "house-9": "Nhà 9 · Niềm tin & mở rộng",
            "house-10": "Nhà 10 · Sự nghiệp & danh tiếng",
            "house-11": "Nhà 11 · Cộng đồng & mục tiêu chung",
            "house-12": "Nhà 12 · Tiềm thức & nội tâm",
          },
          draw: {
            subtitle:
              "Hãy để tâm trí thư thái, tập trung vào câu hỏi của bạn rồi chọn đủ 12 lá bài — mỗi lá sẽ đi vào một ngôi nhà theo thứ tự bạn rút.",
          },
        },
        twelveMonths: {
          title: "Trải bài 12 tháng",
          subtitle:
            "12 lá bài lần lượt ngồi vào 12 tháng liên tiếp, phản ánh năng lượng và diễn biến của bạn trong từng tháng sắp tới.",
          what: {
            title: "Trải bài 12 tháng là gì?",
            body: "Trong trải bài 12 tháng, 12 lá bài được rút ra sẽ tương ứng với 12 tháng liên tiếp tính từ tháng ngay sau khi bạn trải bài. Tháng đầu tiên là tháng kế tiếp, tháng cuối cùng là cùng tháng đó của năm sau, tạo thành một chu kỳ 12 tháng trọn vẹn để bạn lên kế hoạch cho cả năm tới.",
          },
          why: {
            title: "Trải bài này để làm gì?",
            items: {
              forecast:
                "Dự báo theo từng tháng: Thấy rõ năng lượng của từng tháng sẽ tăng hay giảm, thay vì một dự báo chung chung cho cả năm.",
              timing:
                "Chọn đúng thời điểm: Biết tháng nào nên bắt đầu dự án, ký kết, ứng tuyển hoặc nghỉ ngơi để giảm rủi ro.",
              prepare:
                "Chuẩn bị tinh thần: Chuẩn bị sẵn tinh thần và nguồn lực cho những tháng được báo là khó khăn.",
            },
          },
          when: {
            title: "Khi nào bạn nên xem 12 tháng?",
            items: {
              milestone:
                "Khi bước vào một năm mới hoặc một chu kỳ mới: Đầu năm, sinh nhật, hoặc khi bạn muốn lên kế hoạch dài hạn.",
              decision:
                "Khi phải đưa ra quyết định lớn: Chuyển việc, đầu tư, khởi nghiệp, hoặc thay đổi mối quan hệ.",
              quiet:
                "Khi muốn nhìn trước một năm: Dành cho lúc bạn cần sự chắc chắn thay vì phản ứng theo từng sự kiện.",
            },
          },
          positions: {
            title: "12 tháng trong trải bài của bạn",
            hint: "Mỗi lá bài bạn chọn sẽ được giải nghĩa cho đúng tháng tương ứng.",
          },
          preview: {
            title: "Bạn sẽ xem 12 tháng nào?",
            hint: "Tháng đầu tiên là tháng ngay sau tháng bạn trải bài, tháng cuối cùng là cùng tháng đó của năm sau.",
            currentMonth: "Tháng trải bài: {{month}}",
            range: "Tháng 1 · {{first}} → Tháng 12 · {{last}}",
          },
          monthThemes: {
            "month-1": "Khởi đầu, mở màn, năng lượng mới",
            "month-2": "Tiến triển sớm, điều chỉnh cho quen",
            "month-3": "Tăng tốc, kết quả đầu tiên",
            "month-4": "Ổn định, củng cố quý đầu",
            "month-5": "Mở rộng, niềm vui, sáng tạo & tình cảm",
            "month-6": "Cân bằng, nhịp sống, sức khỏe",
            "month-7": "Đối tác, hợp tác, cam kết",
            "month-8": "Thay đổi sâu, tài nguyên chung",
            "month-9": "Tăng trưởng, học tập, mở rộng tầm nhìn",
            "month-10": "Đỉnh cao sự nghiệp, danh tiếng",
            "month-11": "Cộng đồng, bạn bè, mục tiêu chung",
            "month-12": "Khép lại chu kỳ, tích hợp bài học",
          },
          draw: {
            subtitle:
              "Hãy để tâm trí thư thái, tập trung vào chu kỳ 12 tháng tới rồi chọn đủ 12 lá bài — mỗi lá sẽ đi vào một tháng theo thứ tự bạn rút.",
          },
        },
      },
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
      parentTitle: "Lich sử trải bài",
      title: "Lịch sử 1 lá",
      subtitle: "Xem lại các suy nghĩ và góc nhìn vũ trụ đã qua",
      empty: "Chưa có lượt trải bài nào",
      deleteDescription:
        "Trải bài này sẽ bị xóa vĩnh viễn. Bạn có chắc chắn muốn xóa không?",
      deleteTitle: "Xóa lượt trải bài này?",
      deleteConfirm: "Xóa lượt trải bài",
      deleteSuccess: "Đã xóa lượt trải bài",
    },
    historyAiTarot: {
      title: "Lịch sử AI Tarot",
      subtitle: "Xem lại các bài giải AI và góc nhìn vũ trụ đã qua",
      empty: "Chưa có bài giải AI nào",
      deleteTitle: "Xóa bài giải AI này?",
      deleteDescription:
        "Bài giải AI này sẽ bị xóa vĩnh viễn. Bạn có chắc chắn muốn xóa không?",
      deleteConfirm: "Xóa bài giải",
      deleteSuccess: "Đã xóa bài giải AI",
    },
    historyAiDeepTarot: {
      title: "Lịch sử Tarot chuyên sâu",
      subtitle: "Xem lại các trải bài chuyên sâu đã qua",
      empty: "Chưa có trải bài chuyên sâu nào",
      deleteTitle: "Xóa trải bài này?",
      deleteDescription:
        "Trải bài chuyên sâu này sẽ bị xóa vĩnh viễn. Bạn có chắc chắn muốn xóa không?",
      deleteConfirm: "Xóa trải bài",
      deleteSuccess: "Đã xóa trải bài chuyên sâu",
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
    wallet: {
      title: "Ví của tôi",
      subtitle:
        "Theo dõi xu trắng và xu đỏ của bạn. Xu trắng từng đợt sẽ hết hạn sau một khoảng thời gian, hãy chú ý hạn dùng.",
      batchesTitle: "Các đợt xu trắng",
      batchesSubtitle:
        "Phần xu còn lại trong một đợt sẽ bị mất khi đợt hết hạn. Danh sách xếp theo hạn dùng, đợt gần hết hạn hiện trước.",
      empty: "Bạn chưa có đợt xu trắng nào",
      colAmount: "Số xu gốc",
      colRemaining: "Còn lại",
      colExpiresAt: "Hết hạn",
      daysLeft: "Còn {{days}} ngày",
      expiresToday: "Hết hạn hôm nay",
      convertTitle: "Đổi xu đỏ sang xu trắng",
      convertSubtitle:
        "1 xu đỏ = 2 xu trắng. Số xu trắng nhận được sẽ được cộng thành một đợt mới.",
      convertRedCoins: "Số xu đỏ cần đổi",
      convertYouReceive: "Bạn sẽ nhận được",
      convertButton: "Đổi",
      convertSuccess: "Đã đổi {{redCoins}} xu đỏ thành {{whiteCoins}} xu trắng",
      convertExceedsBalance: "Bạn chỉ có {{balance}} xu đỏ",
    },
  },
} as const;
