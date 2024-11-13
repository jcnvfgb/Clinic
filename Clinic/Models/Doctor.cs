namespace Clinic.Models
{
    public class Doctor
    {
        public int DoctorID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int SpecialtyID { get; set; }
        public int CategoryID { get; set; }
        public int RoleID { get; set; }
    }

    public class DoctorDetails
    {
        public int DoctorID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string SpecialtyID { get; set; }
        public string CategoryID { get; set; }
        public string RoleID { get; set; }
    }
}
