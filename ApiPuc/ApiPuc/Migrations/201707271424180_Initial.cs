namespace ApiPuc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Produtoes",
                c => new
                    {
                        idproduto = c.Int(nullable: false, identity: true),
                        nome = c.String(nullable: false),
                        preco = c.String(),
                        prazoentrega = c.String(),
                    })
                .PrimaryKey(t => t.idproduto);
            
            CreateTable(
                "dbo.UsuarioProdutoes",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        idproduto = c.Int(nullable: false),
                        idusuario = c.Int(nullable: false),
                        datacompra = c.String(),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.Usuarios",
                c => new
                    {
                        idusuario = c.Int(nullable: false, identity: true),
                        nome = c.String(nullable: false),
                        senha = c.String(),
                        equipe = c.String(),
                    })
                .PrimaryKey(t => t.idusuario);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Usuarios");
            DropTable("dbo.UsuarioProdutoes");
            DropTable("dbo.Produtoes");
        }
    }
}
