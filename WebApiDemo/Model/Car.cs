using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiDemo.Model
{
    [Table("Car")]
    [Custom("class_car", "HeNan")]
    public class Car
    {

        public Car()
        {
        }
        public Car(string name)
        {
            this.carName = name;
        }

        [Key]
        public int id { get; set; }

        //[ColumnName("汽车名称", "CarName")]
        //[ColumnName("汽车名称")]
        [ColumnName(Name = "CarName",Description = "汽车名称")]
        public string carName { get; set; }
        public DateTime createTime { get; set; }
    }
}
