namespace BackendApp.Helpers
{
    public static class OrderHelper
    {
        public static string GetStatus(int status)
        {
            switch(status)
            {
                case 1: 
                    return "จัดส่งแล้ว";
                default:
                    return "กำลังดำเนินการ";
            }
        }
    }
}