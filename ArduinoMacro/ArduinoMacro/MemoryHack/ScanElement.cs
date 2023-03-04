using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArduinoMacro
{
    public class ScanElement
    {
        public IntPtr Address { get; set; }
        public long Offset { get; set; }
        public object Value { get; set; }
        public object Previous { get; set; }
        public ScanElement(IntPtr address, long offset, object value) 
        {
            this.Address = address;
            this.Offset = offset;
            this.Value = value;
        }



        public ListViewItem ToListViewItem()
        {
            ListViewItem item = new ListViewItem(Address.ToString());
            item.SubItems.Add(Value.ToString());
            item.SubItems.Add(Previous != null ? Previous.ToString() : "");
            return item;
        }
    }
}
