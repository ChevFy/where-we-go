using where_we_go.Models.Enums;

namespace where_we_go.Database;

public static class SeedDataModels
{
    public static class Users
    {
        public static readonly (string UserName, string Email, string Name, string Password, string Role)[] Data =
        [
            ("admin@example.com", "admin@example.com", "Admin User", "AdminPassword123!", "Admin"),
            ("john@example.com", "john@example.com", "John Doe", "JohnPassword123!", "User"),
            ("jane@example.com", "jane@example.com", "Jane Smith", "JanePassword123!", "User"),
            ("alice@example.com", "alice@example.com", "Alice Wong", "AlicePassword123!", "User"),
            ("bob@example.com", "bob@example.com", "Bob Brown", "BobPassword123!", "User"),
            ("charlie@example.com", "charlie@example.com", "Charlie Davis", "CharliePassword123!", "User")
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
        // (Title, Description, Location, DaysUntilDeadline, MinPart, MaxPart, InviteCode, Categories, OwnerEmail, DaysAgo)
        public static readonly (string Title, string Description, string Location, int DaysUntilDeadline, int MinPart, int MaxPart, string InviteCode, string[] Categories, string OwnerEmail, int DaysAgo)[] Data =
        [
            ("Mountain Trip", "Hiking adventure in the north.", "Chiang Mai", 7, 2, 5, "TRIP01", new[] { "Travel", "Nature" }, "alice@example.com", 5),
            ("Cafe Hopping", "Exploring aesthetic cafes.", "Bangkok", 3, 2, 3, "CAFE02", new[] { "Food" }, "jane@example.com", 4),
            ("Beach Day", "Sun, sand, and relaxing vibes.", "Phuket", 10, 4, 10, "BEACH3", new[] { "Nature", "Travel" }, "alice@example.com", 3),
            ("Concert Night", "Live music at local venue.", "Bangkok", 5, 3, 8, "MUSIC4", new[] { "Entertainment" }, "john@example.com", 2),
            ("Shopping Spree", "Weekend shopping at outlet mall.", "Bangkok", 2, 2, 6, "SHOP05", new[] { "Shopping" }, "bob@example.com", 1),
            ("Street Food Tour", "Discovering hidden street food gems.", "Chiang Mai", 4, 3, 7, "FOOD06", new[] { "Food", "Travel" }, "charlie@example.com", 0),
            ("Waterfall Trekking", "Explore beautiful waterfalls and nature trails.", "Kanchanaburi", 14, 4, 12, "TREK07", new[] { "Nature", "Travel" }, "john@example.com", 6),
            ("Movie Marathon", "Back-to-back movies at cinema complex.", "Bangkok", 1, 2, 4, "MOVIE8", new[] { "Entertainment" }, "jane@example.com", 7),
            ("Island Hopping", "Explore beautiful islands and beaches.", "Krabi", 21, 6, 15, "ISLAND9", new[] { "Travel", "Nature" }, "alice@example.com", 10),
            ("Fine Dining Experience", "Luxury dining at rooftop restaurant.", "Bangkok", 6, 2, 4, "DINE10", new[] { "Food", "Entertainment" }, "bob@example.com", 8)
        ];
    }

    public static class Participants
    {
        // (PostIndex, UserEmail, Status, DaysAgo)
        public static readonly (int PostIndex, string UserEmail, ParticipantStatus Status, int DaysAgo)[] Data =
        [
            // Post 0: Mountain Trip (Owner: alice) - john is Pending
            (0, "john@example.com", ParticipantStatus.Pending, 1),
            
            // Post 1: Cafe Hopping (Owner: jane) - alice Approved, bob Pending
            (1, "alice@example.com", ParticipantStatus.Approved, 3),
            (1, "bob@example.com", ParticipantStatus.Pending, 1),
            
            // Post 2: Beach Day (Owner: alice) - john Approved, charlie Rejected
            (2, "john@example.com", ParticipantStatus.Approved, 2),
            (2, "charlie@example.com", ParticipantStatus.Rejected, 1),
            
            // Post 3: Concert Night (Owner: john) - alice Approved
            (3, "alice@example.com", ParticipantStatus.Approved, 1),
            
            // Post 4: Shopping Spree (Owner: bob) - jane Approved
            (4, "jane@example.com", ParticipantStatus.Approved, 0)
        ];
    }

    public static class Notifications
    {
        // (PostIndex, UserEmail, Content, IsRead, Type, Link, DaysAgo)
        public static readonly (int PostIndex, string UserEmail, string Content, bool IsRead, NotificationType Type, string Link, int DaysAgo)[] Data =
        [
            // Post 0: Mountain Trip (Owner: alice) - john requested to join (Pending)
            (0, "alice@example.com", "John Doe requested to join your activity.", false, NotificationType.ParticipantRequested, "/Post/PostDetail", 1),
            
            // Post 1: Cafe Hopping (Owner: jane) - alice was approved, bob is pending
            (1, "alice@example.com", "Your request to join was approved!", true, NotificationType.ParticipantApproved, "/Post/PostDetail", 3),
            (1, "jane@example.com", "Bob Brown requested to join your activity.", false, NotificationType.ParticipantRequested, "/Post/PostDetail", 1),
            
            // Post 2: Beach Day (Owner: alice) - john was approved, charlie was rejected  
            (2, "john@example.com", "Your request to join was approved!", true, NotificationType.ParticipantApproved, "/Post/PostDetail", 2),
            (2, "charlie@example.com", "Your request to join was declined.", false, NotificationType.ParticipantRejected, "/Post/PostDetail", 1),
            
            // Post 3: Concert Night (Owner: john) - alice was approved
            (3, "alice@example.com", "Your request to join was approved!", true, NotificationType.ParticipantApproved, "/Post/PostDetail", 1),
            
            // Post 4: Shopping Spree (Owner: bob) - jane was approved  
            (4, "jane@example.com", "Your request to join was approved!", true, NotificationType.ParticipantApproved, "/Post/PostDetail", 0)
        ];
    }
}