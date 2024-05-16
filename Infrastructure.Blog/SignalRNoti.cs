using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Blog
{
    public class SignalRNoti :Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("Noti", message);
        }
    }
}
