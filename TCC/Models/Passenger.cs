using SQLite;

namespace TCC.Models
{
    [Table("Passengers")]
    public class Passenger
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull, MaxLength(200), Unique]
        public string Name { get; set; }

        [NotNull, MaxLength(200)]
        public string Password { get; set; }

        [NotNull, MaxLength(300), Unique]
        public string Email { get; set; }

        [NotNull, MaxLength(20)]
        public string PhoneNumber { get; set; }

        [NotNull, MaxLength(20)]
        public string EmergencyPhoneNumber { get; set; }

        [NotNull, MaxLength(200)]
        public string Address { get; set; }

        [NotNull, MaxLength(200)]
        public string ReservableAddress { get; set; }

        [NotNull, MaxLength(20), Unique]
        public string RG {  get; set; }

        [NotNull, MaxLength(20), Unique]
        public string CPF { get; set; }

        [NotNull, MaxLength(20)]
        public string Genre { get; set; }

        [NotNull, MaxLength(200)]
        public string School { get; set; }

        [NotNull, MaxLength(200)]
        public string ResponsableName { get; set; }

        [NotNull, MaxLength(200)]
        public bool   SpecialTreatment { get; set; }

        [MaxLength(500)]
        public string? SpecialTreatmentObservations { get; set; }

        [NotNull, MaxLength(200)]
        public DateTime BirthDate { get; set; }

        [NotNull]
        public double Latitude { get; set; } = 0;

        [NotNull]
        public double Longitude { get; set; } = 0;

        public bool EVH { get; set; } = true; // EVH -> "Eu vou hoje"
    }
}
