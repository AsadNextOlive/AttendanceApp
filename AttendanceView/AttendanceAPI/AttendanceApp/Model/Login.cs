namespace AttendanceApp.Model
{
    public class Login
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool isLoggedIn { get; set; }
        public int AccountType { get; set; }
    }
}
