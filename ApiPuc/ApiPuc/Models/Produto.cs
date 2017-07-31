using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ApiPuc.Models
{
    public class Produto
    {
        [Key]
        public int idproduto { get; set; }
        [Required]
        public string nome { get; set; }
        public string preco { get; set; }
        public string prazoentrega { get; set; }
    }
}