using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Ws.Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            var wsOptions = new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(120) };
            app.UseWebSockets(wsOptions);
            app.Use(async (context, next) =>{
                if (context.Request.Path == "/send")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                        {
                            await Send(context, webSocket);
                        }
                    }
                    else {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
            });


        }
        private async Task Send(HttpContext context, WebSocket webSocket)
        {
            
            string data = "";
            string dataString = "";
            var buffer = new byte[372036857];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);
            if (result != null)
            {
                while (!result.CloseStatus.HasValue)
                {
                    string message = Encoding.UTF8.GetString(new ArraySegment<byte>(buffer,0, result.Count));
                    Console.WriteLine($"Message from client: {message}");
                    using (HttpClient client = new HttpClient())
                    {
                        Root raceInformation = new Root();
                        try
                        {
                            var response = await client.GetAsync(message);
                            response.EnsureSuccessStatusCode();
                            if (response.IsSuccessStatusCode)
                            {
                                 data = await response.Content.ReadAsStringAsync();
                                dataString = JsonConvert.SerializeObject(data);
                            }
                            else
                            {
                                //TempData["ErrorMessage"] = ($"Response Error Code: {response.StatusCode}");
                            }
                        }
                        catch (Exception e)
                        {
                            //TempData["ErrorMessage"] = e.ToString();
                        }

                        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(data)), result.MessageType, result.EndOfMessage, System.Threading.CancellationToken.None);
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), System.Threading.CancellationToken.None);
                        Console.WriteLine(result);
                    }
                }
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, System.Threading.CancellationToken.None);
        }
    }
}

public class Circuit
{
    public string circuitId { get; set; }
    public string url { get; set; }
    public string circuitName { get; set; }
    public Location Location { get; set; }
}

public class FirstPractice
{
    public string date { get; set; }
    public string time { get; set; }
}

public class Location
{
    public string lat { get; set; }
    public string @long { get; set; }
    public string locality { get; set; }
    public string country { get; set; }
}

public class MRData
{
    public string xmlns { get; set; }
    public string series { get; set; }
    public string url { get; set; }
    public string limit { get; set; }
    public string offset { get; set; }
    public string total { get; set; }
    public RaceTable RaceTable { get; set; }
}

public class Qualifying
{
    public string date { get; set; }
    public string time { get; set; }
}

public class Race
{
    public string season { get; set; }
    public string round { get; set; }
    public string url { get; set; }
    public string raceName { get; set; }
    public Circuit Circuit { get; set; }
    public string date { get; set; }
    public string time { get; set; }
    public FirstPractice FirstPractice { get; set; }
    public SecondPractice SecondPractice { get; set; }
    public ThirdPractice ThirdPractice { get; set; }
    public Qualifying Qualifying { get; set; }
    public Sprint Sprint { get; set; }
}

public class RaceTable
{
    public string season { get; set; }
    public List<Race> Races { get; set; }
}

public class Root
{
    public MRData MRData { get; set; }
}

public class SecondPractice
{
    public string date { get; set; }
    public string time { get; set; }
}

public class Sprint
{
    public string date { get; set; }
    public string time { get; set; }
}

public class ThirdPractice
{
    public string date { get; set; }
    public string time { get; set; }
}