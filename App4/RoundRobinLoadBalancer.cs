using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App4
{
    class RoundRobinLoadBalancer : LoadBalancer
    {
        Queue<String> serverList;
        
        string server1 = "http://169.254.13.167/";
        string server2 = "http://192.168.1.27/";

        public RoundRobinLoadBalancer()
        {
            serverList = new Queue<string>();
            serverList.Enqueue(server1);
            serverList.Enqueue(server2);
            Debug.WriteLine(serverList.Count);

        }

        //PROBLEMA, EL JPG O PNG ES UN GET TAMBIEN Y SE LO PIDE AL "OTRO" SERVIDOR.
public override string balanceRequests()
        {
            
            serverList.Enqueue(serverList.Peek());
            return serverList.Dequeue();
        }
    }
}
