using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ApiPuc.Models
{
    public class Usuario
    {
        [Key]
        public int idusuario { get; set; }
        [Required]
        public string nome { get; set; }
        public string senha { get; set; }
        public string equipe { get; set; }
    }
}