using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiDemo.Model
{
  [Table("UserInfo")]
  public class User
  {
    [Key]
    public int id;
    public string firstName;
    public string lastName;
  }
}
