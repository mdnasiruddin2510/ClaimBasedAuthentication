using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimBasedAuthentication.Application.ViewModel
{
    public class VmSelectList
    {
        public VmSelectList(IEnumerable items, string dataValueField, string dataTextField)
        {
            Items = items;
            DataValueField = dataValueField;
            DataTextField = dataTextField;
        }

        public string DataTextField { get; }
        public string DataValueField { get; }
        public IEnumerable Items { get; }
    }
}
