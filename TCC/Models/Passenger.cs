using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCC.Models
{
    internal class Passenger
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string EmergencyPhoneNumber { get; set; }
        public string Address { get; set; }
        public string ReservableAddress { get; set; }
        public string RG {  get; set; }
        public string CPF { get; set; }
        public string School { get; set; }
        public string ResponsableName { get; set; }
        public bool   SpecialTreatment { get; set; }
        public string? SpecialTreatmentObservations { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
