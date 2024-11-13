using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Clinic.Models
{
    public class Visit
    {
        public int VisitID { get; set; }
        public int DoctorID { get; set; }
        public int PatientID { get; set; }
        public DateTime VisitDate { get; set; }
        public string Diagnosis { get; set; }
        public double TreatmentCost { get; set; }
        public int VisitTypeID { get; set; }
    }

    public class VisitDetails
    {
        public int VisitID { get; set; }
        public string DoctorName { get; set; }
        public string DoctorMiddleName { get; set; }
        public string PatientLastName { get; set; }
        public string PatientName { get; set; }
        public string PatientMiddleName { get; set; }
        public DateTime VisitDate { get; set; }
        public string Diagnosis { get; set; }
        public double TreatmentCost { get; set; }
        public string VisitTypeName { get; set; }
    }
}
