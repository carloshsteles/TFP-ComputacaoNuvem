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

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace ApiPuc.Controllers
{
    public class UsuariosProdutosController : ApiController
    {

        private ApiPucContext db = new ApiPucContext();


        static CloudQueue cloudQueueOne;

        // Connection to QueueOne
        public static void ConnectToStorageQueue()
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=storageposgraduacao;AccountKey=EZ4Ea+feAr+nCVpsZ5rYuJCw2xSsDspv6Z7Is7k/BA86oleQQnOdlUIp2xglqwX/7uQoBs1VjVumhenpEtAE8A==;EndpointSuffix=core.windows.net";
            CloudStorageAccount cloudStorageAccount;

            if (!CloudStorageAccount.TryParse(connectionString, out cloudStorageAccount))
            {
                Console.WriteLine("Expected connection string 'Azure Storage Account to be a valid Azure Storage Connection String.");
            }

            var cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();

            cloudQueueOne = cloudQueueClient.GetQueueReference("queueone");
            cloudQueueOne.CreateIfNotExists();

        }

        //PUT message in QueueOne
        private string PutMessageToQueueOne(String MessageText)
        {
            ConnectToStorageQueue();
            var message = new CloudQueueMessage(MessageText);
            cloudQueueOne.AddMessage(message);
            return "Messagem adicionada com sucesso na fila QueueOne";

        }

        public void SendMessageToFirebase(String message)
        {

            // Create a request using a URL that can receive a post. 
            WebRequest request = WebRequest.Create("http://fcm.googleapis.com/fcm/send");
            // Set the Method property of the request to POST.
            request.Method = "POST";
            // Create POST data and convert it to a byte array.
            string postData = message;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", "key=AAAAn5ZvqF0:APA91bEuDgVaqp78r1HiBQ5Y2SNULpO2VIvHSD2CI9jiI6k7eHi3JGcayH62UzJe_sGiIJBHFxskVIzipFXRvweVOyOcNSX3UOLlb7Ladn75VmKqDAu-8G64nFXQxn7tP_0NhaBF10cd");
            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;
            // Get the request stream.
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.
            dataStream.Close();
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            Console.WriteLine(responseFromServer);
            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();

            Trace.TraceInformation(responseFromServer);
        }

        
        // GET api/UsuariosProdutos
        public IQueryable<UsuarioProduto> GetUsuarioProdutoes()
        {
            return db.UsuarioProdutoes;
        }

        // GET api/UsuariosProdutos/5
        [ResponseType(typeof(UsuarioProduto))]
        public async Task<IHttpActionResult> GetUsuarioProduto(int id)
        {
            UsuarioProduto usuarioproduto = await db.UsuarioProdutoes.FindAsync(id);
            if (usuarioproduto == null)
            {
                return NotFound();
            }

            return Ok(usuarioproduto);
        }

        // PUT api/UsuariosProdutos/5
        public async Task<IHttpActionResult> PutUsuarioProduto(int id, UsuarioProduto usuarioproduto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != usuarioproduto.id)
            {
                return BadRequest();
            }

            db.Entry(usuarioproduto).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioProdutoExists(id))
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

        // POST api/UsuariosProdutos
        [ResponseType(typeof(UsuarioProduto))]
        public async Task<IHttpActionResult> PostUsuarioProduto(UsuarioProduto usuarioproduto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.UsuarioProdutoes.Add(usuarioproduto);
            await db.SaveChangesAsync();

            Usuario usuario = await db.Usuarios.FindAsync(usuarioproduto.idusuario);
            Produto produto = await db.Produtoes.FindAsync(usuarioproduto.idproduto);
            
            System.Text.StringBuilder messageOne = new System.Text.StringBuilder();

            messageOne.Append("{");
            messageOne.Append("\"condition\": \"'Papaleguas' in topics || 'Coiote' in topics\",");
            messageOne.Append("\"notification\":");
            messageOne.Append("{");
            messageOne.Append("\"title\": \"Pedido processado com Sucesso\",");
            messageOne.Append("\"body\": \"A equipe " + usuario.equipe + " comprou um(a) " + produto.nome + "\"");
            messageOne.Append("},");
            messageOne.Append("\"sound\": \"mysound\"");
            messageOne.Append("}");

            PutMessageToQueueOne(messageOne.ToString());

            System.Text.StringBuilder messageTwo = new System.Text.StringBuilder();

            messageTwo.Append("{");
            messageTwo.Append("\"condition\": \"'Papaleguas' in topics || 'Coiote' in topics\",");
            messageTwo.Append("\"notification\":");
            messageTwo.Append("{");
            messageTwo.Append("\"title\": \"Compra realizada no Sistema\",");
            messageTwo.Append("\"body\": \"A equipe " + usuario.equipe + " comprou um(a) " + produto.nome + "\"");
            messageTwo.Append("},");
            messageTwo.Append("\"sound\": \"mysound\"");
            messageTwo.Append("}");

            SendMessageToFirebase(messageTwo.ToString());
            
            return CreatedAtRoute("DefaultApi", new { id = usuarioproduto.id }, usuarioproduto);
        }

        // DELETE api/UsuariosProdutos/5
        [ResponseType(typeof(UsuarioProduto))]
        public async Task<IHttpActionResult> DeleteUsuarioProduto(int id)
        {
            UsuarioProduto usuarioproduto = await db.UsuarioProdutoes.FindAsync(id);
            if (usuarioproduto == null)
            {
                return NotFound();
            }

            db.UsuarioProdutoes.Remove(usuarioproduto);
            await db.SaveChangesAsync();

            return Ok(usuarioproduto);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UsuarioProdutoExists(int id)
        {
            return db.UsuarioProdutoes.Count(e => e.id == id) > 0;
        }
    }
}