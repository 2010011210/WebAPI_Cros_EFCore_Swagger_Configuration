using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiDemo.Model
{
  [Table("Car")]
  public class Car
  {
    [Key]
    public int id { get; set; }
    public string carName { get; set; }
    public DateTime createTime { get; set; }
  }
}
