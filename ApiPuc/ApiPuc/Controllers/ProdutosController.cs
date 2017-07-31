using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using ApiPuc.Models;

namespace ApiPuc.Controllers
{
    public class ProdutosController : ApiController
    {
        private ApiPucContext db = new ApiPucContext();

        // GET api/Produtos
        public IQueryable<Produto> GetProdutoes()
        {
            return db.Produtoes;
        }

        // GET api/Produtos/5
        [ResponseType(typeof(Produto))]
        public async Task<IHttpActionResult> GetProduto(int id)
        {
            Produto produto = await db.Produtoes.FindAsync(id);
            if (produto == null)
            {
                return NotFound();
            }

            return Ok(produto);
        }

        // PUT api/Produtos/5
        public async Task<IHttpActionResult> PutProduto(int id, Produto produto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != produto.idproduto)
            {
                return BadRequest();
            }

            db.Entry(produto).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProdutoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/Produtos
        [ResponseType(typeof(Produto))]
        public async Task<IHttpActionResult> PostProduto(Produto produto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Produtoes.Add(produto);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = produto.idproduto }, produto);
        }

        // DELETE api/Produtos/5
        [ResponseType(typeof(Produto))]
        public async Task<IHttpActionResult> DeleteProduto(int id)
        {
            Produto produto = await db.Produtoes.FindAsync(id);
            if (produto == null)
            {
                return NotFound();
            }

            db.Produtoes.Remove(produto);
            await db.SaveChangesAsync();

            return Ok(produto);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProdutoExists(int id)
        {
            return db.Produtoes.Count(e => e.idproduto == id) > 0;
        }
    }
}