using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CityInfo.API.Entities
{
    public class PointOfInterest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "You should provide the name value")]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string Description { get; set; }

        // It would also work in this case by a convension without setting the [ForeignKey] attribute, 
        // but if we change the name of the property it would not work anymore
        [ForeignKey(nameof(CityId))] 
        public City City { get; set; }

        public int CityId { get; set; }
    }
}