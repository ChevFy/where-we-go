namespace where_we_go.Models.Enums
{
    public enum ParticipantStatus
    {
        Pending, // รอคัดเลือก
        Approved, // ได้รับเลือก
        Rejected, // ไม่ได้รับเลือก
        Withdrawn // ถอนตัวหลังจากได้รับเลือกแล้ว 
    }
}