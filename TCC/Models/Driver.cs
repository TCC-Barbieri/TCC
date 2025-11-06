using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace TCC.Models
{
    [Table("Drivers")]
    public class Driver
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull, MaxLength(200), Unique]
        public string Name { get; set; }

        [NotNull, MaxLength(200)]
        public string Password { get; set; }

        [NotNull, MaxLength(300), Unique]
        public string Email { get; set; }

        [NotNull, MaxLength(20), Unique]
        public string PhoneNumber { get; set; }

        [NotNull, MaxLength(20), Unique]
        public string EmergencyPhoneNumber { get; set; }

        [NotNull, MaxLength(200)]
        public string Address { get; set; }

        [NotNull, MaxLength(20), Unique]
        public string RG { get; set; }

        [NotNull, MaxLength(20), Unique]
        public string CPF { get; set; }

        [NotNull, MaxLength(100), Unique]
        public string CNH { get; set; }

        [NotNull, MaxLength(20)]
        public string Genre { get; set; }

        [NotNull, MaxLength(200)]
        public DateTime BirthDate { get; set; }

        [NotNull]
        public double Latitude { get; set; }
        [NotNull]   
        public double Longitude { get; set; }
    }
}
