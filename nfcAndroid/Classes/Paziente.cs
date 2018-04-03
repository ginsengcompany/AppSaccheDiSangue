using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using nfcAndroid.Service;

namespace nfcAndroid.Classes
{
    public class Paziente
    {
        public string uid { get; set; }
        public string nome { get; set; }
        public string cognome { get; set; }
        public string gruppo { get; set; }
        public string rh { get; set; }
        public string uidSacca { get; set; }

        public async Task<bool> getPaziente(Context context)
        {
            REST<object, Paziente> rest = new REST<object, Paziente>();
            List<Header> headers = new List<Header>();
            headers.Add(new Header("uid", this.uid));
            var response = await rest.GetSingleJson("http://192.168.125.14:3000/pazienti/take", headers);
            if (rest.responseMessage != HttpStatusCode.OK)
            {
                Toast.MakeText(context, rest.warning, ToastLength.Short).Show();
                return false;
            }
            this.nome = response.nome;
            this.cognome = response.cognome;
            this.gruppo = response.gruppo;
            this.rh = response.rh;
            return true;
        }

    }
}