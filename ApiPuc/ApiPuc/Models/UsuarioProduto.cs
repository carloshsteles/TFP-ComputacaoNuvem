using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ApiPuc.Models
{
    public class UsuarioProduto
    {
        [Key]
        public int id { get; set; }
        [Required]
        public int idproduto { get; set; }
        public int idusuario { get; set; }
        public string datacompra { get; set; }
    }
}