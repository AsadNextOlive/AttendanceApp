using System.Text.RegularExpressions;

namespace AttendanceApp.Service
{
    public class ValidationService
    {
        public bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[\w\.-]+@[\w\.-]+\.\w+$");
        }
        public bool IsAlphanumeric(string input)
        {
            // Check for at least one letter, one digit, and one of the specified special symbols
            return Regex.IsMatch(input, @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@!#$%&*]).+$");
        }
    }
}
