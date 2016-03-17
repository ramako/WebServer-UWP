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


        public RoundRobinLoadBalancer()
        {
            serverList = new Queue<string>();
           
            foreach(var item in ConfigData.getIps())
            {
                serverList.Enqueue("http://" + item.Text+"/");
            }
        }

public override string balanceRequests()
        {
            
            serverList.Enqueue(serverList.Peek());
            return serverList.Dequeue();
        }
    }
}
