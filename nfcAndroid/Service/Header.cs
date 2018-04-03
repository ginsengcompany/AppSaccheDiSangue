using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace nfcAndroid.Service
{
    public class Header
    {
        public string header;
        public string value;

        public Header() { }

        public Header(string header, string value)
        {
            this.header = header;
            this.value = value;
        }
    }
}