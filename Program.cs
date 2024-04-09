using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace TenorAPIExample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/api/{id}", async context =>
                            {
                                // Define la URL de la API de Tenor para la búsqueda de GIFs emocionantes
                                string apiKey = "LIVDSRZULELA";
                                string searchQuery = "excited";
                                int limit = 50; // Limita los resultados a 50 GIFs
                                string url = $"https://g.tenor.com/v1/search?q={searchQuery}&key={apiKey}&limit={limit}";

                                // Crea un cliente HTTP para hacer la solicitud a la API de Tenor
                                using (HttpClient client = new HttpClient())
                                {
                                    try
                                    {
                                        // Realiza la solicitud GET a la API de Tenor y espera la respuesta
                                        HttpResponseMessage response = await client.GetAsync(url);

                                        // Verifica si la solicitud fue exitosa
                                        if (response.IsSuccessStatusCode)
                                        {
                                            // Lee la respuesta como una cadena JSON
                                            string responseBody = await response.Content.ReadAsStringAsync();

                                            // Deserializa la respuesta JSON en un objeto
                                            dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);

                                            // Obtiene el GIF correspondiente al índice especificado en la URL
                                            int id = int.Parse(context.Request.RouteValues["id"].ToString());
                                            dynamic gif = jsonResponse.results[id];

                                            // Crea un objeto JSON con los datos del GIF seleccionado
                                            var selectedGif = new
                                            {
                                                text = gif.id,
                                                href = gif.media[0].webm.preview
                                            };

                                            // Devuelve la respuesta en formato JSON
                                            context.Response.Headers.Add("Content-Type", "application/json");
                                            await context.Response.WriteAsync(JsonConvert.SerializeObject(selectedGif));
                                        }
                                        else
                                        {
                                            context.Response.StatusCode = (int)response.StatusCode;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        context.Response.StatusCode = 500;
                                        await context.Response.WriteAsync($"Error al hacer la solicitud: {ex.Message}");
                                    }
                                }
                            });
                        });
                    });
                });
    }
}
