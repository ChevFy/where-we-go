
namespace where_we_go.Models.Enums
{
    public enum NotificationType
    {
        ActivityCancelled, // กิจกรรมถูกยกเลิก (send to all participants except rejected and withdrawn)
        ParticipantRequested, // มีคนขอเข้าร่วมกิจกรรม (send to owner when user request to join)
        ParticipantApproved, // ได้รับเลือกเข้าร่วมกิจกรรม (send to user when status changed to approved)
        ParticipantRejected, // ไม่ได้รับเลือกเข้าร่วมกิจกรรม (send to user when status changed to rejected)
        ParticipantWithdrawn, // ถอนตัวจากกิจกรรม (send to user when status changed to withdrawn)
        PostCompleted, // กิจกรรมเสร็จสิ้น (send to all participants when post status changed to completed)

        // expired and participant full (meet minimum required)
        // send to all participants when post status changed to expired and participant full (meet minimum required)
        PostExpiredFull, // หมดเวลารับสมัครแต่จำนวนผู้เข้าร่วมครบ (ส่งให้ทุกคนเมื่อสถานะโพสต์เปลี่ยนเป็นหมดเวลารับสมัครและจำนวนผู้เข้าร่วมครบ) 

        // not full but expired (doesn't meet minimum participant)
        // cancelled (send to all participants when post status changed to expired but not meet minimum required)
        PostExpiredNotFull, // หมดเวลารับสมัครแต่จำนวนผู้เข้าร่วมไม่ครบ (ส่งให้ทุกคนเมื่อสถานะโพสต์เปลี่ยนเป็นหมดเวลารับสมัครแต่จำนวนผู้เข้าร่วมไม่ครบ)
    }
}