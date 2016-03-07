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
        //TODO change to a data structure
        string server1 = "http://169.254.13.167/";
        string server2 = "http://192.168.1.15/";

        public RoundRobinLoadBalancer()
        {
            serverList = new Queue<string>();
            serverList.Enqueue(server1);
            serverList.Enqueue(server2);

        }

public override string balanceRequests()
        {
            
            serverList.Enqueue(serverList.Peek());
            return serverList.Dequeue();
        }
    }
}
