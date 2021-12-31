using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
namespace Server
{
  public  class SettingsServer
    {
        public int Version { get; set; }
        public int MaxThreadPool { get; set; }
        public int MaxAsyncThreadPool { get; set; }
    }
}
