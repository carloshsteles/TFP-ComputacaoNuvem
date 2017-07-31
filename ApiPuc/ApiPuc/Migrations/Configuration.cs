namespace ApiPuc.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using ApiPuc.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<ApiPuc.Models.ApiPucContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ApiPuc.Models.ApiPucContext context)
        {
            context.Usuarios.AddOrUpdate(x => x.idusuario,
                new Usuario() { idusuario = 1, nome = "admin", senha = "admin", equipe = "Papaleguas" },
                new Usuario() { idusuario = 2, nome = "Flavio", senha = "123456", equipe = "Coiote" },
                new Usuario() { idusuario = 3, nome = "Pedro", senha = "123456", equipe = "Papaleguas" }
                );

            context.Produtoes.AddOrUpdate(x => x.idproduto,
                new Produto() { idproduto = 1, nome = "Carabina", prazoentrega = "10 dias", preco = "100" },
                new Produto() { idproduto = 2, nome = "Machado", prazoentrega = "10 dias", preco = "250" },
                new Produto() { idproduto = 3, nome = "Espada", prazoentrega = "7 dias", preco = "300" },
                new Produto() { idproduto = 4, nome = "Corda", prazoentrega = "5 dias", preco = "40" },
                new Produto() { idproduto = 5, nome = "Arco e Flecha", prazoentrega = "8 dias", preco = "60" },
                new Produto() { idproduto = 6, nome = "Escudo", prazoentrega = "12 dias", preco = "180" },
                new Produto() { idproduto = 7, nome = "Estilingue", prazoentrega = "6 dias", preco = "20" },
                new Produto() { idproduto = 8, nome = "Catapulta", prazoentrega = "20 dias", preco = "500" },
                new Produto() { idproduto = 9, nome = "Armadilha de Urso", prazoentrega = "10 dias", preco = "120" },
                new Produto() { idproduto = 10, nome = "Espingarda", prazoentrega = "10 dias", preco = "380" }
                );
        }
    }
}
