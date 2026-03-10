namespace where_we_go.Models.Enums
{
    public enum PostStatus
    {
        Open, // กำลังเปิดรับสมัคร
        Full, // ครบจำนวนแล้ว (ปิดรับอัตโนมัติ)
        Closed, // เจ้าของกดปิดรับสมัครเอง
        Cancelled, // กิจกรรมถูกยกเลิก
        Completed // กิจกรรมจบลงแล้ว (เลยเวลา Expiry) 
    }
}
