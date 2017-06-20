using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MaptionaryTests {
    class TestServer {
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";

        public string response = "";

        private readonly HttpListener _listener = new HttpListener();

        public TestServer(string host) {
            if (!HttpListener.IsSupported) {
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");
            }

            _listener.Prefixes.Add(host);
            _listener.Start();
        }

        public void Run() {
            ThreadPool.QueueUserWorkItem((o) => {
                Console.WriteLine("Webserver running...");
                try {
                    while(_listener.IsListening) {
                        ThreadPool.QueueUserWorkItem((c) => {
                            var ctx = c as HttpListenerContext;
                            try {
                                byte[] buf = Encoding.UTF8.GetBytes(response);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch {
                                // Suppress any exceptions
                            }
                            finally {
                                ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch {
                    //Supress
                }
            });
        }

        public void Stop() {
            _listener.Stop();
            _listener.Close();
        }
    }
}
