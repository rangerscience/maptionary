using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Threading;

//TODO: Think about ways to check binding validity (a bool conversion, maybe?)
namespace Maptionary {
    public class Bind {

        public string source {
            get;
            private set;
        }

        public Node data {
            get;
            private set;
        }


        public delegate void Del(Node n);

        private FileSystemWatcher watcher;

        public Bind(string source) {
            if(source.StartsWith("http", true, null)) {
                BindToHTTP(source);
            } else {
                BindToFile(source);
            }
        }

        private void BindToHTTP(string url) {
            source = url;

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(source);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            data = Parser.Parse(
                (new StreamReader(response.GetResponseStream())).ReadToEnd()
            );


            //TODO: Parameterize refresh
            //TODO: Manually trigger refresh
            //TODO: Bookkeepiong on refresh
            //TODO: Better than infinite loop :P
            ThreadPool.QueueUserWorkItem((o) => {
                while(true) {
                    Thread.Sleep(5000); //5 sec refresh
                    //TODO: Copypasta
                    HttpWebRequest rq = (HttpWebRequest)WebRequest.Create(source);
                    HttpWebResponse rs = (HttpWebResponse)rq.GetResponse();

                    data = Parser.Parse(
                        (new StreamReader(rs.GetResponseStream())).ReadToEnd()
                    );
                }
            });
        }

        //TODO: Better way(s) to wait for file access :P
        private const int NumRetries = 5;
        private const int DelayOnRetry = 100;
        private void BindToFile(string s) {
            source = Path.GetFullPath(s);

            data = Parser.Parse(
                System.IO.File.ReadAllText(source)
            );

            // FSW appears to only take directories, not individual files
            // TODO: Let you Bind(directory), otherwise, you'll have a BUNCH of FSW triggering on files in a data directory :P

            watcher = new FileSystemWatcher(Path.GetDirectoryName(source));
            watcher.NotifyFilter = NotifyFilters.LastWrite;

            watcher.Changed += this.Update;

            watcher.EnableRaisingEvents = true;
        }
        private void Update(object sender, FileSystemEventArgs e) {
            if (e.FullPath == source) {
                for (int i = 0; i < NumRetries; i++) {
                    try {
                        this.data = Parser.Parse(
                            System.IO.File.ReadAllText(source)
                        );
                        break;
                    }
                    catch (IOException err) {

                        if(i == NumRetries) {
                            throw;
                        }

                        System.Threading.Thread.Sleep(DelayOnRetry);
                    }
                }
            }
        }
    };
}
