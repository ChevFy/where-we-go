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
        public static readonly (string Title, string Description, string Location, int DaysUntilDeadline, int MinPart, int MaxPart, string InviteCode, string[] Categories)[] Data =
        [
            ("Mountain Trip", "Hiking adventure in the north.", "Chiang Mai", 7, 2, 5, "TRIP01", new[] { "Travel", "Nature" }),
            ("Cafe Hopping", "Exploring aesthetic cafes.", "Bangkok", 3, 2, 3, "CAFE02", new[] { "Food" }),
            ("Beach Day", "Sun, sand, and relaxing vibes.", "Phuket", 10, 4, 10, "BEACH3", new[] { "Nature", "Travel" }),
            ("Concert Night", "Live music at local venue.", "Bangkok", 5, 3, 8, "MUSIC4", new[] { "Entertainment" }),
            ("Shopping Spree", "Weekend shopping at outlet mall.", "Bangkok", 2, 2, 6, "SHOP05", new[] { "Shopping" }),
            ("Street Food Tour", "Discovering hidden street food gems.", "Chiang Mai", 4, 3, 7, "FOOD06", new[] { "Food", "Travel" }),
            ("Waterfall Trekking", "Explore beautiful waterfalls and nature trails.", "Kanchanaburi", 14, 4, 12, "TREK07", new[] { "Nature", "Travel" }),
            ("Movie Marathon", "Back-to-back movies at cinema complex.", "Bangkok", 1, 2, 4, "MOVIE8", new[] { "Entertainment" }),
            ("Island Hopping", "Explore beautiful islands and beaches.", "Krabi", 21, 6, 15, "ISLAND9", new[] { "Travel", "Nature" }),
            ("Fine Dining Experience", "Luxury dining at rooftop restaurant.", "Bangkok", 6, 2, 4, "DINE10", new[] { "Food", "Entertainment" })
        ];
    }

    public static class Participants
    {
        // PostIndex, UserEmails (participants for that post)
        public static readonly (int PostIndex, string[] UserEmails)[] Data =
        [
            // Post 0: Mountain Trip (Min: 2, Max: 5) - Less than min (1 participant)
            (0, new[] { "john@example.com" }),
            
            // Post 1: Cafe Hopping (Min: 2, Max: 3) - Full (3 participants)
            (1, new[] { "jane@example.com", "alice@example.com", "bob@example.com" }),
            
            // Post 2: Beach Day (Min: 4, Max: 10) - Equal to min (4 participants)
            (2, new[] { "john@example.com", "jane@example.com", "charlie@example.com", "alice@example.com" })
        ];
    }
}