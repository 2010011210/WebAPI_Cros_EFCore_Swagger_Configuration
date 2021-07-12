using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiDemo.Model
{
  /// <summary>
  /// location information
  /// </summary>
  [Table("LocationInfo")]
  public class LocationInfo
  {
    [Key]
    public int id { get; set; }
    //[Key]
    //public int id { get; set; }
    /// <summary>
    /// name
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// address
    /// </summary>
    public string address { get; set; }
  }
}
