using where_we_go.Models.Enums;

namespace where_we_go.Database;

public static class SeedDataModels
{
    public static class Users
    {
        // (UserName, Email, Name, Password, Role, ProfileImageKey)
        public static readonly (string UserName, string Email, string Name, string Password, string Role, string? ProfileImageKey)[] Data =
        [
            ("admin@example.com", "admin@example.com", "Admin User", "AdminPassword123!", "Admin", "/images/profiles/admin.jpg"),
            ("opor", "opor@example.com", "Opor The Coke Devourer", "OporPassword123!", "User", "/images/profiles/opor.png"),
            ("arnon", "arnon@example.com", "Arnon The Sleeping Beauty", "ArnomPassword123!", "User", "/images/profiles/arnon.png"),
            ("paaw", "paaw@example.com", "Paaw The Street Food Explorer", "PaawPassword123!", "User", "/images/profiles/paaw.jpg"),
            ("owen", "owen@example.com", "Owen The Competitive Racist", "OwenPassword123!", "User", "/images/profiles/owen.png"),
            ("zard", "zard@example.com", "Zard Hero of the south", "ZardPassword123!", "User", "/images/profiles/zard.jpg"),
            ("wai", "wai@example.com", "Wai The Sausage Seeker", "WaiPassword123!", "User", "/images/profiles/wai.jpg"),
            ("min", "min@example.com", "Min The Kid(s)naper", "MinPassword123!", "User", "/images/profiles/min.jpg"),
            ("opor2", "opor2@example.com", "Opor The Great Gooner Of Khonkaen", "Opor2Password123!", "User", "/images/profiles/opor2.jpg"),
            ("chevfy", "chevfy@example.com", "Chevfy The Wife Abuser", "ChevfyPassword123!", "User", "/images/profiles/chevfy.jpg"),
            ("pluem", "pluem@example.com", "Pluem The Confuser", "PluemPassword123!", "User", "/images/profiles/pluem.jpg"),
            ("shogun", "shogun@example.com", "Shogun The Gunsho", "ShogunPassword123!", "User", "/images/profiles/shogun.jpg")
        ];
    }

    public static class Categories
    {
        public static readonly (string Name, string Description)[] Data =
        [
            ("Travel", "Travel destinations and experiences"),
            ("Food", "Dining and food recommendations"),
            ("Entertainment", "Movies, music, and entertainment venues"),
            ("Shopping", "Shopping malls and stores"),
            ("Nature", "Parks, hiking, and outdoor activities")
        ];
    }

    public static class Posts
    {
        // (Title, Description, Location, DaysUntilDeadline, MinPart, MaxPart, InviteCode, Categories, OwnerEmail, DaysAgo, lat, lon, picture, MinutesUntilDeadline)
        public static readonly (string Title, string Description, string Location, int DaysUntilDeadline, int MinPart, int MaxPart, string InviteCode, string[] Categories, string OwnerEmail, int DaysAgo, double lat, double lon, string picture, int? MinutesUntilDeadline)[] Data =
        [
            ("Mountain Trip", "Hiking adventure in the north.", "Chiang Mai", 7, 2, 5, "TRIP01", new[] { "Travel", "Nature" }, "opor@example.com", 0, 27.988838193540236, 86.92575288928171, "/images/posts/mountain_trip.jpg", 2),
            ("Cafe Hopping", "Exploring aesthetic cafes.", "Bangkok", 3, 2, 3, "CAFE02", new[] { "Food" }, "arnon@example.com", 0, 13.718691950404612, 100.80257934296758, "/images/posts/cafe_hopping.jpg", 4),
            ("Beach Day", "Sun, sand, and relaxing vibes.", "Phuket", 10, 4, 10, "BEACH3", new[] { "Nature", "Travel" }, "paaw@example.com", 0, 13.284856271400894, 100.91415000870191, "/images/posts/beach_day.jpg", 6),
            ("Concert Night", "Live music at local venue.", "Bangkok", 5, 3, 8, "MUSIC4", new[] { "Entertainment" }, "owen@example.com", 0, 13.725075614607055, 100.76368387596173, "/images/posts/concert_night.jpg", 8),
            ("Shopping Spree", "Weekend shopping at outlet mall.", "Bangkok", 2, 2, 6, "SHOP05", new[] { "Shopping" }, "zard@example.com", 0, 13.721175171749776, 100.7248345848456, "/images/posts/shopping_spree.jpg", 10),
            ("Street Food Tour", "Discovering hidden street food gems.", "Chiang Mai", 1, 1, 7, "FOOD06", new[] { "Food", "Travel" }, "wai@example.com", 1, 13.927166047675854, 100.62778211922259, "/images/posts/street_food_tour.jpg", 1),
            ("Waterfall Trekking", "Explore beautiful waterfalls and nature trails.", "Kanchanaburi", 1, 4, 10, "TREK07", new[] { "Nature", "Travel" }, "min@example.com", 5, 14.726052243399694, 101.18916352591403, "/images/posts/waterfall_trekking.jpg", null),
            ("Movie Marathon", "Back-to-back movies at cinema complex.", "Bangkok", 1, 2, 4, "MOVIE8", new[] { "Entertainment" }, "opor2@example.com", 5, 13.758888703064654, 100.72378325299546, "/images/posts/movie_marathon.jpg", null),
            ("Island Hopping", "Explore beautiful islands and beaches.", "Krabi", 1, 6, 15, "ISLAND9", new[] { "Travel", "Nature" }, "chevfy@example.com", 5, 8.086311441674644, 98.90625465870191, "/images/posts/island_hopping.jpg", null),
            ("Fine Dining Experience", "Luxury dining at rooftop restaurant.", "Bangkok", 6, 2, 4, "DINE10", new[] { "Food", "Entertainment" }, "pluem@example.com", 2, 13.730042, 100.54127, "/images/posts/fine_dining.jpg", null)
        ];
    }

    public static class Participants
    {
        // (InviteCode, UserEmail, Status)
        public static readonly (string InviteCode, string UserEmail, ParticipantStatus Status)[] Data =
        [
            // Post 0: Mountain Trip (InviteCode: TRIP01) owner: opor
            ("TRIP01", "arnon@example.com", ParticipantStatus.Pending),
            ("TRIP01", "paaw@example.com", ParticipantStatus.Approved),
            ("TRIP01", "owen@example.com", ParticipantStatus.Approved),
            ("TRIP01", "zard@example.com", ParticipantStatus.Rejected),
            ("TRIP01", "wai@example.com", ParticipantStatus.Pending),
            
            // Post 1: Cafe Hopping (InviteCode: CAFE02) owner: arnon
            ("CAFE02", "owen@example.com", ParticipantStatus.Approved),
            ("CAFE02", "zard@example.com", ParticipantStatus.Approved),
            ("CAFE02", "paaw@example.com", ParticipantStatus.Approved),
            ("CAFE02", "min@example.com", ParticipantStatus.Rejected),
            ("CAFE02", "sho@example.com", ParticipantStatus.Withdrawn),
            
            // Post 2: Beach Day (InviteCode: BEACH3) owner: paaw
            ("BEACH3", "wai@example.com", ParticipantStatus.Approved),
            ("BEACH3", "min@example.com", ParticipantStatus.Approved),
            ("BEACH3", "opor@example.com", ParticipantStatus.Approved),
            ("BEACH3", "arnon@example.com", ParticipantStatus.Approved),
            ("BEACH3", "zard@example.com", ParticipantStatus.Approved),
            ("BEACH3", "opor2@example.com", ParticipantStatus.Approved),
            ("BEACH3", "pluem@example.com", ParticipantStatus.Pending),

            
            // Post 3: Concert Night (InviteCode: MUSIC4) owner: owen
            ("MUSIC4", "opor2@example.com", ParticipantStatus.Pending),
            ("MUSIC4", "opor@example.com", ParticipantStatus.Pending),
            ("MUSIC4", "arnon@example.com", ParticipantStatus.Pending),
            ("MUSIC4", "paaw@example.com", ParticipantStatus.Pending),
            ("MUSIC4", "zard@example.com", ParticipantStatus.Pending),
            ("MUSIC4", "wai@example.com", ParticipantStatus.Pending),
            ("MUSIC4", "min@example.com", ParticipantStatus.Pending),
            ("MUSIC4", "chevfy@example.com", ParticipantStatus.Pending),
            
            // Post 4: Shopping Spree (InviteCode: SHOP05) owner: zard
            ("SHOP05", "chevfy@example.com", ParticipantStatus.Pending),
            ("SHOP05", "opor@example.com", ParticipantStatus.Pending),
            ("SHOP05", "arnon@example.com", ParticipantStatus.Pending),
            ("SHOP05", "paaw@example.com", ParticipantStatus.Rejected),
            ("SHOP05", "owen@example.com", ParticipantStatus.Pending),
            ("SHOP05", "wai@example.com", ParticipantStatus.Pending),
            
            // Post 5: Street Food Tour (InviteCode: FOOD06) owner: wai
            ("FOOD06", "opor@example.com", ParticipantStatus.Approved),
            ("FOOD06", "arnon@example.com", ParticipantStatus.Approved),
            ("FOOD06", "paaw@example.com", ParticipantStatus.Approved),
            ("FOOD06", "owen@example.com", ParticipantStatus.Approved),
            // Post 6: Waterfall Trekking (InviteCode: TREK07) owner: min
            
            // Post 7: Movie Marathon (InviteCode: MOVIE8)
            
            // Post 8: Island Hopping (InviteCode: ISLAND9)
            
            // Post 9: Fine Dining Experience (InviteCode: DINE10)
        ];
    }

    public static class Notifications
    {
        // (PostIndex, UserEmail, Content, IsRead, Type, Link, DaysAgo)
        public static readonly (int PostIndex, string UserEmail, string Content, bool IsRead, NotificationType Type, string Link, int DaysAgo)[] Data =
        [
            // Post 0: Mountain Trip
            (0, "opor@example.com", "Arnon The Sleeping Beauty requested to join your activity.", false, NotificationType.ParticipantRequested, "/Post/PostDetail", 1),
            (0, "paaw@example.com", "Your request to join was approved!", true, NotificationType.ParticipantApproved, "/Post/PostDetail", 2),
            (0, "owen@example.com", "Your request to join was approved!", true, NotificationType.ParticipantApproved, "/Post/PostDetail", 3),
            (0, "zard@example.com", "Your request to join was declined.", true, NotificationType.ParticipantRejected, "/Post/PostDetail", 2),
            (0, "opor@example.com", "Wai The Sausage Seeker requested to join your activity.", false, NotificationType.ParticipantRequested, "/Post/PostDetail", 1),
            (0, "min@example.com", "Your request to join was approved!", true, NotificationType.ParticipantApproved, "/Post/PostDetail", 4),
            (0, "opor@example.com", "Opor The Great Gooner Of Khonkaen requested to join your activity.", false, NotificationType.ParticipantRequested, "/Post/PostDetail", 0),
            (0, "chevfy@example.com", "Your request to join was approved!", true, NotificationType.ParticipantApproved, "/Post/PostDetail", 3)
        ];
    }
}