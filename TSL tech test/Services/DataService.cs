using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TSL_tech_test.Services
{
    public class DataService
    {


        public async Task<string> GetDataFromServerAsync(string[] args, string season, string circuitName)
        {
            //formats the url to receive the data from an external source
            if (!string.IsNullOrEmpty(circuitName))
                circuitName = "circuits/" + circuitName;
            

            string serversMessage = "did not work";

            //initialise websocket and some details
            using (ClientWebSocket client = new ClientWebSocket())
            {
                Uri serviceUri = new Uri("ws://localhost:58790/send");
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(120));
                //try to connect to the server
                try
                {
                    await client.ConnectAsync(serviceUri, cts.Token);
                    var n = 0;
                    //message it sends to the server is the formatted url for the external data source
                    string message = $"https://ergast.com/api/f1/{season + circuitName}.json";

                    //while the clients has an open connection with the server
                    while (client.State == WebSocketState.Open)
                    {
                        //check that the message is not null - would always have a value but good for future proofing
                        if (!string.IsNullOrEmpty(message))
                        {
                            //variables that will go with the request
                            ArraySegment<byte> byteToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                            await client.SendAsync(byteToSend, WebSocketMessageType.Text, true, cts.Token);
                            var responseBuffer = new byte[37203685];
                            var offset = 0;
                            var packetSize = 37203685;
                            //while the message is still sending and hasn't been fully sent yet
                            while (true)
                            {
                                //send message and wait for server to respond, then exit and return the data to the client
                                ArraySegment<byte> byteRecieved = new ArraySegment<byte>(responseBuffer, offset, packetSize);
                                WebSocketReceiveResult response = await client.ReceiveAsync(byteRecieved, cts.Token);
                                var responseMessage = Encoding.UTF8.GetString(responseBuffer, offset, response.Count);
                                serversMessage = responseMessage;
                                if (response.EndOfMessage)
                                    break;
                            }
                            return serversMessage;
                        }
                    }
                }
                catch (WebSocketException e)
                {
                    return e.ToString();
                }
                return serversMessage;
            }
        }
    }
}
