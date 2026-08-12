using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QueueApp.Api.Models;

[Table("QueueSettings")]
public class QueueSetting
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; } = 1;

    public int CurrentIndex { get; set; } = -1;

    public DateTime LastActive { get; set; } = DateTime.UtcNow;
}
